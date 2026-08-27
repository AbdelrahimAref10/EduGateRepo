using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetMyLessonCounts;

public sealed record GetMyLessonCountsQuery(int UserId)
    : IRequest<Result<TeacherLessonCountsDto>>;

public sealed class GetMyLessonCountsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyLessonCountsQuery, Result<TeacherLessonCountsDto>>
{
    public async Task<Result<TeacherLessonCountsDto>> Handle(
        GetMyLessonCountsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<TeacherLessonCountsDto>.NotFound("Teacher profile was not found.");

        var byYear = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .GroupBy(x => new { x.AcademicYearId, x.AcademicYear.Name })
            .Select(g => new TeacherLessonAcademicYearCountDto
            {
                AcademicYearId = g.Key.AcademicYearId,
                AcademicYearName = g.Key.Name,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return Result<TeacherLessonCountsDto>.Success(new TeacherLessonCountsDto
        {
            Total = byYear.Sum(x => x.Count),
            ByAcademicYear = byYear
        });
    }
}
