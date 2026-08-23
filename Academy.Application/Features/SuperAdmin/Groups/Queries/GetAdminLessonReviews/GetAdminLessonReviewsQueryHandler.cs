using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminLessonReviews;

public sealed class GetAdminLessonReviewsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAdminLessonReviewsQuery, Result<AdminReviewsDto>>
{
    public async Task<Result<AdminReviewsDto>> Handle(
        GetAdminLessonReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId)
            .Select(x => new { x.Id, x.RatingCount, x.RatingAverage })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            return Result<AdminReviewsDto>.NotFound("الدرس غير موجود.");

        var items = await dbContext.LessonReviews
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId)
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
            TargetId = lesson.Id,
            Count = lesson.RatingCount,
            Average = lesson.RatingAverage,
            Items = items
        });
    }
}
