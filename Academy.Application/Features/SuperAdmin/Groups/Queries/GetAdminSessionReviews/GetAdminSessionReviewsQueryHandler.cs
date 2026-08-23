using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminSessionReviews;

public sealed class GetAdminSessionReviewsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAdminSessionReviewsQuery, Result<AdminReviewsDto>>
{
    public async Task<Result<AdminReviewsDto>> Handle(
        GetAdminSessionReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.Id == request.SessionId)
            .Select(x => new { x.Id, x.RatingCount, x.RatingAverage })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            return Result<AdminReviewsDto>.NotFound("الحصة غير موجودة.");

        var items = await dbContext.SessionReviews
            .AsNoTracking()
            .Where(x => x.LessonGroupSessionId == request.SessionId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AdminReviewDto
            {
                Id = x.Id,
                StudentId = x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                StudentCode = x.Student.StudentCode,
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Result<AdminReviewsDto>.Success(new AdminReviewsDto
        {
            TargetId = session.Id,
            Count = session.RatingCount,
            Average = session.RatingAverage,
            Items = items
        });
    }
}
