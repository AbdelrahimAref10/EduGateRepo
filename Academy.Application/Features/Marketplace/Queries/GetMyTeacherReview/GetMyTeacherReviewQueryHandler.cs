using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace.Queries.GetMyTeacherReview;

public sealed class GetMyTeacherReviewQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyTeacherReviewQuery, Result<MyTeacherReviewDto>>
{
    public async Task<Result<MyTeacherReviewDto>> Handle(
        GetMyTeacherReviewQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<MyTeacherReviewDto>.NotFound("Student profile was not found.");

        var teacherExists = await dbContext.Teachers
            .AnyAsync(x => x.Id == request.TeacherId, cancellationToken);

        if (!teacherExists)
            return Result<MyTeacherReviewDto>.NotFound("Teacher was not found.");

        var canReview = await dbContext.LessonBookings.AnyAsync(
            x => x.StudentId == student.Id
                && x.TeacherId == request.TeacherId
                && x.Status == BookingStatus.Confirmed,
            cancellationToken);

        var review = await dbContext.TeacherReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TeacherId == request.TeacherId && x.StudentId == student.Id,
                cancellationToken);

        return Result<MyTeacherReviewDto>.Success(new MyTeacherReviewDto
        {
            CanReview = canReview,
            Review = review is null ? null : MarketplaceMappings.ToDto(review)
        });
    }
}
