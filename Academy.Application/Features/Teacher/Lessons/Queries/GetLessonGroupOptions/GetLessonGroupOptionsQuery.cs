using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroupOptions;

public sealed record GetLessonGroupOptionsQuery(int UserId, int LessonId)
    : IRequest<Result<IReadOnlyList<LessonGroupOptionDto>>>;

public sealed class GetLessonGroupOptionsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLessonGroupOptionsQuery, Result<IReadOnlyList<LessonGroupOptionDto>>>
{
    public async Task<Result<IReadOnlyList<LessonGroupOptionDto>>> Handle(
        GetLessonGroupOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LessonGroupOptionDto>>.NotFound("Teacher profile was not found.");

        var lessonOk = await dbContext.Lessons.AnyAsync(
            x => x.Id == request.LessonId && x.TeacherId == teacherId.Value,
            cancellationToken);

        if (!lessonOk)
            return Result<IReadOnlyList<LessonGroupOptionDto>>.NotFound("الدرس غير موجود.");

        var items = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new LessonGroupOptionDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LessonGroupOptionDto>>.Success(items);
    }
}
