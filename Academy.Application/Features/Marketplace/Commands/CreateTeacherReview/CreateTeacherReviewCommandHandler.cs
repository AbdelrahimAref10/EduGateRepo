using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace.Commands.CreateTeacherReview;

public sealed class CreateTeacherReviewCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<CreateTeacherReviewCommand, Result<TeacherReviewDto>>
{
    public async Task<Result<TeacherReviewDto>> Handle(
        CreateTeacherReviewCommand request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<TeacherReviewDto>.NotFound("Student profile was not found.");

        var teacher = await dbContext.Teachers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.TeacherId, cancellationToken);

        if (teacher is null)
            return Result<TeacherReviewDto>.NotFound("Teacher was not found.");

        var confirmed = await dbContext.LessonBookings.AnyAsync(
            x => x.StudentId == student.Id
                && x.TeacherId == teacher.Id
                && x.Status == BookingStatus.Confirmed,
            cancellationToken);

        if (!confirmed)
            return Result<TeacherReviewDto>.Failure(
                "You can only review a teacher after a confirmed booking.");

        var exists = await dbContext.TeacherReviews.AnyAsync(
            x => x.TeacherId == teacher.Id && x.StudentId == student.Id,
            cancellationToken);

        if (exists)
            return Result<TeacherReviewDto>.Conflict("You already reviewed this teacher.");

        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        var review = new TeacherReview
        {
            TeacherId = teacher.Id,
            StudentId = student.Id,
            Rating = request.Rating,
            Comment = comment,
            CreatedAtUtc = DateTime.UtcNow
        };

        var ratings = await dbContext.TeacherReviews
            .Where(x => x.TeacherId == teacher.Id)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);
        ratings.Add(review.Rating);
        TeacherRatingCalculator.Apply(teacher, ratings);

        dbContext.TeacherReviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [teacher.UserId],
                UserTargetId = student.UserId,
                Type = NotificationType.TeacherReviewReceived,
                EntityType = NotificationEntityType.Teacher,
                EntityId = teacher.Id,
                TitleAr = "تقييم جديد",
                TitleEn = "New review",
                BodyAr = $"الطالب {student.User.FullName} قيّمك بـ {request.Rating} من 5.",
                BodyEn = $"Student {student.User.FullName} rated you {request.Rating} out of 5.",
                IncludeSuperAdmins = false
            },
            cancellationToken);

        return Result<TeacherReviewDto>.Success(MarketplaceMappings.ToDto(review));
    }
}
