using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Reviews.Queries.GetMySessionReview;

public sealed class GetMySessionReviewQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMySessionReviewQuery, Result<MyTargetReviewDto>>
{
    public async Task<Result<MyTargetReviewDto>> Handle(
        GetMySessionReviewQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<MyTargetReviewDto>.NotFound("Student profile was not found.");

        var session = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.Id == request.SessionId)
            .Select(x => new { x.Id, x.LessonGroupId, x.StartedAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            return Result<MyTargetReviewDto>.NotFound("الحصة غير موجودة.");

        var isMember = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.LessonGroupId == session.LessonGroupId && x.StudentId == student.Id,
                cancellationToken);

        var canReview = isMember && session.StartedAtUtc.HasValue;

        var review = await dbContext.SessionReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.LessonGroupSessionId == request.SessionId && x.StudentId == student.Id,
                cancellationToken);

        return Result<MyTargetReviewDto>.Success(new MyTargetReviewDto
        {
            CanReview = canReview,
            Review = review is null ? null : ReviewMappings.ToDto(review)
        });
    }
}
