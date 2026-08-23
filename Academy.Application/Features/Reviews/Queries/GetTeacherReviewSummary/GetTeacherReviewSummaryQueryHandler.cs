using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Reviews.Queries.GetTeacherReviewSummary;

public sealed class GetTeacherReviewSummaryQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherReviewSummaryQuery, Result<TeacherReviewSummaryDto>>
{
    public async Task<Result<TeacherReviewSummaryDto>> Handle(
        GetTeacherReviewSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<TeacherReviewSummaryDto>.NotFound("Teacher profile was not found.");

        var teacher = await StatAsync(
            dbContext.TeacherReviews.AsNoTracking().Where(x => x.TeacherId == teacherId).Select(x => x.Rating),
            cancellationToken);
        var lessons = await StatAsync(
            dbContext.LessonReviews.AsNoTracking().Where(x => x.TeacherId == teacherId).Select(x => x.Rating),
            cancellationToken);
        var sessions = await StatAsync(
            dbContext.SessionReviews.AsNoTracking().Where(x => x.TeacherId == teacherId).Select(x => x.Rating),
            cancellationToken);
        var allCount = teacher.Count + lessons.Count + sessions.Count;
        var allAverage = allCount == 0
            ? 0d
            : ((double)teacher.Average * teacher.Count
               + (double)lessons.Average * lessons.Count
               + (double)sessions.Average * sessions.Count) / allCount;

        return Result<TeacherReviewSummaryDto>.Success(new TeacherReviewSummaryDto
        {
            Teacher = teacher,
            Lessons = lessons,
            Sessions = sessions,
            All = ReviewMappings.ToStat(allCount, allAverage)
        });
    }

    private static async Task<ReviewStatDto> StatAsync(
        IQueryable<int> ratings,
        CancellationToken cancellationToken)
    {
        var row = await ratings
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Average = g.Average() })
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? ReviewMappings.ToStat(0, 0)
            : ReviewMappings.ToStat(row.Count, row.Average);
    }
}
