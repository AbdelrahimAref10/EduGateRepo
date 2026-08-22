using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Reviews.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Reviews.Queries.GetMyLessonReview;

public sealed class GetMyLessonReviewQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyLessonReviewQuery, Result<MyTargetReviewDto>>
{
    public async Task<Result<MyTargetReviewDto>> Handle(
        GetMyLessonReviewQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<MyTargetReviewDto>.NotFound("Student profile was not found.");

        var lessonExists = await dbContext.Lessons
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.LessonId, cancellationToken);

        if (!lessonExists)
            return Result<MyTargetReviewDto>.NotFound("الدرس غير موجود.");

        var canReview = await dbContext.LessonBookings
            .AsNoTracking()
            .AnyAsync(
                x => x.StudentId == student.Id
                     && x.LessonId == request.LessonId
                     && x.Status == BookingStatus.Confirmed,
                cancellationToken);

        var review = await dbContext.LessonReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.LessonId == request.LessonId && x.StudentId == student.Id,
                cancellationToken);

        return Result<MyTargetReviewDto>.Success(new MyTargetReviewDto
        {
            CanReview = canReview,
            Review = review is null ? null : ReviewMappings.ToDto(review)
        });
    }
}
