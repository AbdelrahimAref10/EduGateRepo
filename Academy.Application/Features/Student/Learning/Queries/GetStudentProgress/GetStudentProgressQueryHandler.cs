using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.LearningPath.Common;
using Academy.Application.Features.LearningPath.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Learning.Queries.GetStudentProgress;

public sealed record GetStudentProgressQuery(int UserId) : IRequest<Result<ProgressReportDto>>;

public sealed class GetStudentProgressQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStudentProgressQuery, Result<ProgressReportDto>>
{
    public async Task<Result<ProgressReportDto>> Handle(
        GetStudentProgressQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && !x.IsParent)
            .Select(x => new { x.Id, Name = x.User.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
            return Result<ProgressReportDto>.NotFound("Student profile was not found.");

        var names = new Dictionary<int, string> { [student.Id] = student.Name };
        var lessons = await LearningPathQueries.BuildProgressAsync(
            dbContext, [student.Id], names, teacherId: null, lessonId: null, cancellationToken);

        return Result<ProgressReportDto>.Success(new ProgressReportDto { Lessons = lessons });
    }
}
