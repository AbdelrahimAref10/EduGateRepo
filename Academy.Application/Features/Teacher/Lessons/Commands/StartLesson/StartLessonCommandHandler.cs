using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Commands.StartLesson;

public sealed class StartLessonCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<StartLessonCommand, Result<LessonDto>>
{
    public async Task<Result<LessonDto>> Handle(
        StartLessonCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonDto>.NotFound("Teacher profile was not found.");

        var lesson = await dbContext.Lessons
            .AsTracking()
            .Include(x => x.EducationType)
            .Include(x => x.EducationStage)
            .Include(x => x.EducationYear)
            .Include(x => x.EducationSubject)
            .Include(x => x.Country)
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
            .Include(x => x.Groups)
            .Include(x => x.Bookings)
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (lesson is null)
            return Result<LessonDto>.NotFound("Lesson was not found.");

        if (!lesson.IsActive)
            return Result<LessonDto>.Failure("Cannot start an inactive lesson.");

        if (!lesson.StartedAtUtc.HasValue)
        {
            lesson.StartedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var hasStartedGroup = lesson.Groups.Any(g => g.StartedAtUtc.HasValue);

        return Result<LessonDto>.Success(LessonMappings.ToLessonDto(
            lesson,
            lesson.Groups.Count,
            lesson.Bookings.Count,
            lesson.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
            hasStartedGroup,
            requestLanguage.Current));
    }
}
