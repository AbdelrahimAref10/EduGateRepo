using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Reviews.Queries.GetTeacherReviewInbox;

public sealed class GetTeacherReviewInboxQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherReviewInboxQuery, Result<TeacherReviewInboxDto>>
{
    public async Task<Result<TeacherReviewInboxDto>> Handle(
        GetTeacherReviewInboxQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<TeacherReviewInboxDto>.NotFound("Teacher profile was not found.");

        var skip = Math.Max(0, request.Skip);
        var take = Math.Clamp(request.Take, 1, 50);
        var kind = request.Kind;
        var id = teacherId.Value;

        var teacherQuery = dbContext.TeacherReviews
            .AsNoTracking()
            .Where(x => x.TeacherId == id)
            .Select(x => new TeacherReviewInboxItemDto
            {
                Id = x.Id,
                Kind = (int)ReviewInboxKind.Teacher,
                Rating = x.Rating,
                Comment = x.Comment,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                StudentPhotoUrl = x.Student.User.ProfilePhoto,
                StudentCode = x.Student.StudentCode,
                Subject = null,
                GroupName = null,
                SessionDate = null,
                StartTime = null,
                Topic = null,
                LessonId = null,
                LessonGroupId = null,
                SessionId = null,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            });

        var lessonQuery = dbContext.LessonReviews
            .AsNoTracking()
            .Where(x => x.TeacherId == id)
            .Select(x => new TeacherReviewInboxItemDto
            {
                Id = x.Id,
                Kind = (int)ReviewInboxKind.Lesson,
                Rating = x.Rating,
                Comment = x.Comment,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                StudentPhotoUrl = x.Student.User.ProfilePhoto,
                StudentCode = x.Student.StudentCode,
                Subject = x.Lesson.Subject,
                GroupName = null,
                SessionDate = null,
                StartTime = null,
                Topic = null,
                LessonId = x.LessonId,
                LessonGroupId = null,
                SessionId = null,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            });

        var sessionQuery = dbContext.SessionReviews
            .AsNoTracking()
            .Where(x => x.TeacherId == id)
            .Select(x => new TeacherReviewInboxItemDto
            {
                Id = x.Id,
                Kind = (int)ReviewInboxKind.Session,
                Rating = x.Rating,
                Comment = x.Comment,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                StudentPhotoUrl = x.Student.User.ProfilePhoto,
                StudentCode = x.Student.StudentCode,
                Subject = x.LessonGroupSession.LessonGroup.Lesson.Subject,
                GroupName = x.LessonGroupSession.LessonGroup.Name,
                SessionDate = x.LessonGroupSession.SessionDate,
                StartTime = x.LessonGroupSession.StartTime,
                Topic = x.LessonGroupSession.Topic,
                LessonId = x.LessonGroupSession.LessonGroup.LessonId,
                LessonGroupId = x.LessonGroupSession.LessonGroupId,
                SessionId = x.LessonGroupSessionId,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            });

        var query = kind switch
        {
            ReviewInboxKind.Teacher => teacherQuery,
            ReviewInboxKind.Lesson => lessonQuery,
            ReviewInboxKind.Session => sessionQuery,
            _ => teacherQuery.Concat(lessonQuery).Concat(sessionQuery)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Result<TeacherReviewInboxDto>.Success(new TeacherReviewInboxDto
        {
            Total = total,
            Items = items
        });
    }
}
