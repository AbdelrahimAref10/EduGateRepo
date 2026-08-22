using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.UpdateLessonGroup;

public sealed class UpdateLessonGroupCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateLessonGroupCommand, Result<LessonGroupDto>>
{
    public async Task<Result<LessonGroupDto>> Handle(
        UpdateLessonGroupCommand request,
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

        var group = await dbContext.LessonGroups
            .AsTracking()
            .Include(x => x.Lesson)
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
            .Include(x => x.Dates)
            .Include(x => x.Sessions)
            .Include(x => x.Members)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.TeacherId == teacher.Id,
                cancellationToken);

        if (group is null)
            return Result<LessonGroupDto>.NotFound("Group was not found.");

        if (group.StartedAtUtc.HasValue)
            return Result<LessonGroupDto>.Conflict("لا يمكن تعديل المجموعة بعد بدايتها.");

        if (request.MaxCapacity.HasValue && request.MaxCapacity.Value < group.Members.Count)
            return Result<LessonGroupDto>.Failure("Max capacity cannot be less than current members count.");

        var datesError = LessonMappings.TryBuildDates(request.Dates, out var newDates);
        if (datesError is not null)
            return Result<LessonGroupDto>.Failure(datesError);

        var sessionsError = LessonMappings.TryBuildSessions(
            request.PeriodStartDate,
            request.PeriodEndDate,
            newDates,
            out var newSessions);
        if (sessionsError is not null)
            return Result<LessonGroupDto>.Failure(sessionsError);

        var area = await dbContext.Areas
            .Include(x => x.City)
            .FirstOrDefaultAsync(
                x => x.Id == request.AreaId && x.IsActive && x.CityId == teacherCityId,
                cancellationToken);

        if (area is null)
            return Result<LessonGroupDto>.Failure("Selected area was not found or does not belong to your city.");

        group.Name = request.Name.Trim();
        group.AreaId = area.Id;
        group.Address = request.Address.Trim();
        group.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        group.MaxCapacity = request.MaxCapacity;
        group.PeriodStartDate = request.PeriodStartDate;
        group.PeriodEndDate = request.PeriodEndDate;

        dbContext.LessonGroupDates.RemoveRange(group.Dates);
        dbContext.LessonGroupSessions.RemoveRange(group.Sessions);
        group.Dates = newDates;
        group.Sessions = newSessions;

        await dbContext.SaveChangesAsync(cancellationToken);

        group.Area = area;

        return Result<LessonGroupDto>.Success(
            LessonMappings.ToGroupDto(group, requestLanguage.Current));
    }
}
