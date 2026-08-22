using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace.Commands.UpdateTeacherReview;

public sealed class UpdateTeacherReviewCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateTeacherReviewCommand, Result<TeacherReviewDto>>
{
    public async Task<Result<TeacherReviewDto>> Handle(
        UpdateTeacherReviewCommand request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<TeacherReviewDto>.NotFound("Student profile was not found.");

        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.Id == request.TeacherId, cancellationToken);

        if (teacher is null)
            return Result<TeacherReviewDto>.NotFound("Teacher was not found.");

        var review = await dbContext.TeacherReviews
            .FirstOrDefaultAsync(
                x => x.TeacherId == teacher.Id && x.StudentId == student.Id,
                cancellationToken);

        if (review is null)
            return Result<TeacherReviewDto>.NotFound("Review was not found.");

        review.Rating = request.Rating;
        review.Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        review.UpdatedAtUtc = DateTime.UtcNow;

        var ratings = await dbContext.TeacherReviews
            .Where(x => x.TeacherId == teacher.Id && x.Id != review.Id)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);
        ratings.Add(review.Rating);
        TeacherRatingCalculator.Apply(teacher, ratings);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<TeacherReviewDto>.Success(MarketplaceMappings.ToDto(review));
    }
}
