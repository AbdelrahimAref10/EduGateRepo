using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Reviews.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Reviews.Commands.UpsertLessonReview;

public sealed class UpsertLessonReviewCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<UpsertLessonReviewCommand, Result<TargetReviewDto>>
{
    public async Task<Result<TargetReviewDto>> Handle(
        UpsertLessonReviewCommand request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<TargetReviewDto>.NotFound("Student profile was not found.");

        var lesson = await dbContext.Lessons
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);

        if (lesson is null)
            return Result<TargetReviewDto>.NotFound("الدرس غير موجود.");

        var confirmed = await dbContext.LessonBookings.AnyAsync(
            x => x.StudentId == student.Id
                 && x.LessonId == lesson.Id
                 && x.Status == BookingStatus.Confirmed,
            cancellationToken);

        if (!confirmed)
            return Result<TargetReviewDto>.Failure("يمكنك تقييم الدرس بعد تأكيد الحجز فقط.");

        var review = await dbContext.LessonReviews
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.LessonId == lesson.Id && x.StudentId == student.Id,
                cancellationToken);

        var created = review is null;
        if (review is null)
        {
            review = new LessonReview
            {
                LessonId = lesson.Id,
                TeacherId = lesson.TeacherId,
                StudentId = student.Id,
                Rating = request.Rating,
                Comment = ReviewMappings.TrimComment(request.Comment),
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.LessonReviews.Add(review);
        }
        else
        {
            review.Rating = request.Rating;
            review.Comment = ReviewMappings.TrimComment(request.Comment);
            review.UpdatedAtUtc = DateTime.UtcNow;
        }

        var ratings = await dbContext.LessonReviews
            .Where(x => x.LessonId == lesson.Id && x.Id != review.Id)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);
        ratings.Add(review.Rating);
        ReviewMappings.Apply(lesson, ratings);

        await dbContext.SaveChangesAsync(cancellationToken);

        if (created)
        {
            var teacherUserId = await dbContext.Teachers
                .Where(x => x.Id == lesson.TeacherId)
                .Select(x => x.UserId)
                .FirstAsync(cancellationToken);

            await notificationService.CreateAsync(
                new NotificationCreateRequest
                {
                    RecipientUserIds = [teacherUserId],
                    UserTargetId = student.UserId,
                    Type = NotificationType.LessonReviewReceived,
                    EntityType = NotificationEntityType.Lesson,
                    EntityId = lesson.Id,
                    TitleAr = "تقييم جديد على درس",
                    TitleEn = "New lesson review",
                    BodyAr = $"الطالب {student.User.FullName} قيّم درس {lesson.Subject} بـ {request.Rating} من 5.",
                    BodyEn = $"Student {student.User.FullName} rated {lesson.Subject} {request.Rating} out of 5.",
                    IncludeSuperAdmins = false
                },
                cancellationToken);
        }

        return Result<TargetReviewDto>.Success(ReviewMappings.ToDto(review));
    }
}
