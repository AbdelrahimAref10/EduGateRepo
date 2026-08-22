using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.CreateLessonGroup;

public sealed class CreateLessonGroupCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateLessonGroupCommand, Result<LessonGroupDto>>
{
    public async Task<Result<LessonGroupDto>> Handle(
        CreateLessonGroupCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(x => x.User)
                .ThenInclude(x => x.Area!)
                    .ThenInclude(x => x.City)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonGroupDto>.NotFound("Teacher profile was not found.");

        if (teacher.User.AreaId is null || teacher.User.Area?.City is null)
            return Result<LessonGroupDto>.Failure("Teacher must have a city assigned.");

        var teacherCityId = teacher.User.Area.CityId;

        var lesson = await dbContext.Lessons
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (lesson is null)
            return Result<LessonGroupDto>.NotFound("Lesson was not found.");

        var datesError = LessonMappings.TryBuildDates(request.Dates, out var dates);
        if (datesError is not null)
            return Result<LessonGroupDto>.Failure(datesError);

        var sessionsError = LessonMappings.TryBuildSessions(
            request.PeriodStartDate,
            request.PeriodEndDate,
            dates,
            out var sessions);
        if (sessionsError is not null)
            return Result<LessonGroupDto>.Failure(sessionsError);

        var areaId = request.AreaId ?? lesson.AreaId;

        var area = await dbContext.Areas
            .Include(x => x.City)
            .FirstOrDefaultAsync(
                x => x.Id == areaId && x.IsActive && x.CityId == teacherCityId,
                cancellationToken);

        if (area is null)
            return Result<LessonGroupDto>.Failure("Selected area was not found or does not belong to your city.");

        var group = new LessonGroup
        {
            LessonId = lesson.Id,
            Name = request.Name.Trim(),
            AreaId = area.Id,
            Address = request.Address.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            MaxCapacity = request.MaxCapacity,
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            CreatedAtUtc = DateTime.UtcNow,
            Dates = dates,
            Sessions = sessions,
            Members = []
        };

        dbContext.LessonGroups.Add(group);
        await dbContext.SaveChangesAsync(cancellationToken);

        group.Area = area;

        return Result<LessonGroupDto>.Success(
            LessonMappings.ToGroupDto(group, requestLanguage.Current));
    }
}
