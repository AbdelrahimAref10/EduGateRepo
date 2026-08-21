using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonManage;

public sealed class GetLessonManageQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonManageQuery, Result<LessonManageDto>>
{
    public async Task<Result<LessonManageDto>> Handle(
        GetLessonManageQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<LessonManageDto>.NotFound("Teacher profile was not found.");

        var lesson = await dbContext.Lessons
            .Include(x => x.EducationType)
            .Include(x => x.EducationStage)
            .Include(x => x.EducationYear)
            .Include(x => x.EducationSubject)
            .Include(x => x.Country)
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
            .Include(x => x.Bookings)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Area)
                    .ThenInclude(x => x.City)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Dates)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Sessions)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Members)
                    .ThenInclude(x => x.Student)
                        .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (lesson is null)
            return Result<LessonManageDto>.NotFound("Lesson was not found.");

        var assignments = lesson.Groups
            .SelectMany(g => g.Members.Select(m => new { m.StudentId, Group = g }))
            .ToDictionary(x => x.StudentId, x => x.Group);

        var students = lesson.Bookings
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x =>
            {
                assignments.TryGetValue(x.StudentId, out var group);
                return new LessonStudentDto
                {
                    BookingId = x.Id,
                    StudentId = x.StudentId,
                    StudentName = x.Student.User.FullName,
                    StudentCode = x.Student.StudentCode,
                    Status = x.Status.ToString(),
                    CreatedAtUtc = x.CreatedAtUtc,
                    ReviewedAtUtc = x.ReviewedAtUtc,
                    AssignedGroupId = group?.Id,
                    AssignedGroupName = group?.Name
                };
            })
            .ToList();

        var language = requestLanguage.Current;

        var groups = lesson.Groups
            .OrderBy(x => x.CreatedAtUtc)
            .Select(g => LessonMappings.ToGroupDto(g, language))
            .ToList();

        var hasStartedGroup = lesson.Groups.Any(g => g.StartedAtUtc.HasValue);

        var lessonDto = LessonMappings.ToLessonDto(
            lesson,
            groups.Count,
            lesson.Bookings.Count,
            lesson.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
            hasStartedGroup,
            language);

        return Result<LessonManageDto>.Success(new LessonManageDto
        {
            Lesson = lessonDto,
            Students = students,
            Groups = groups
        });
    }
}
