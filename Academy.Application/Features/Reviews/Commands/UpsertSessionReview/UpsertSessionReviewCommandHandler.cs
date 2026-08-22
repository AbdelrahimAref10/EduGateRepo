using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Reviews.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Reviews.Commands.UpsertSessionReview;

public sealed class UpsertSessionReviewCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<UpsertSessionReviewCommand, Result<TargetReviewDto>>
{
    public async Task<Result<TargetReviewDto>> Handle(
        UpsertSessionReviewCommand request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<TargetReviewDto>.NotFound("Student profile was not found.");

        var session = await dbContext.LessonGroupSessions
            .AsTracking()
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
            .FirstOrDefaultAsync(x => x.Id == request.SessionId, cancellationToken);

        if (session is null)
            return Result<TargetReviewDto>.NotFound("الحصة غير موجودة.");

        if (!session.StartedAtUtc.HasValue)
            return Result<TargetReviewDto>.Failure("يمكنك تقييم الحصة بعد بدء الحصة.");

        var isMember = await dbContext.LessonGroupMembers.AnyAsync(
            x => x.LessonGroupId == session.LessonGroupId && x.StudentId == student.Id,
            cancellationToken);

        if (!isMember)
            return Result<TargetReviewDto>.Failure("يمكنك تقييم الحصة إذا كنت عضوًا في المجموعة.");

        var lesson = session.LessonGroup.Lesson;
        var review = await dbContext.SessionReviews
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.LessonGroupSessionId == session.Id && x.StudentId == student.Id,
                cancellationToken);

        var created = review is null;
        if (review is null)
        {
            review = new SessionReview
            {
                LessonGroupSessionId = session.Id,
                TeacherId = lesson.TeacherId,
                StudentId = student.Id,
                Rating = request.Rating,
                Comment = ReviewMappings.TrimComment(request.Comment),
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.SessionReviews.Add(review);
        }
        else
        {
            review.Rating = request.Rating;
            review.Comment = ReviewMappings.TrimComment(request.Comment);
            review.UpdatedAtUtc = DateTime.UtcNow;
        }

        var ratings = await dbContext.SessionReviews
            .Where(x => x.LessonGroupSessionId == session.Id && x.Id != review.Id)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);
        ratings.Add(review.Rating);
        ReviewMappings.Apply(session, ratings);

        await dbContext.SaveChangesAsync(cancellationToken);

        if (created)
        {
            var teacherUserId = await dbContext.Teachers
                .Where(x => x.Id == lesson.TeacherId)
                .Select(x => x.UserId)
                .FirstAsync(cancellationToken);

            var date = session.SessionDate.ToString("yyyy-MM-dd");
            await notificationService.CreateAsync(
                new NotificationCreateRequest
                {
                    RecipientUserIds = [teacherUserId],
                    UserTargetId = student.UserId,
                    Type = NotificationType.SessionReviewReceived,
                    EntityType = NotificationEntityType.Session,
                    EntityId = session.Id,
                    TitleAr = "تقييم جديد على حصة",
                    TitleEn = "New session review",
                    BodyAr = $"الطالب {student.User.FullName} قيّم حصة {lesson.Subject} بتاريخ {date} بـ {request.Rating} من 5.",
                    BodyEn = $"Student {student.User.FullName} rated the {lesson.Subject} session on {date} {request.Rating} out of 5.",
                    IncludeSuperAdmins = false
                },
                cancellationToken);
        }

        return Result<TargetReviewDto>.Success(ReviewMappings.ToDto(review));
    }
}
