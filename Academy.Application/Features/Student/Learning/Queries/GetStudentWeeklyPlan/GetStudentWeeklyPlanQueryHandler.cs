using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.LearningPath.Common;
using Academy.Application.Features.LearningPath.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Learning.Queries.GetStudentWeeklyPlan;

public sealed record GetStudentWeeklyPlanQuery(int UserId) : IRequest<Result<WeeklyLearningPlanDto>>;

public sealed class GetStudentWeeklyPlanQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStudentWeeklyPlanQuery, Result<WeeklyLearningPlanDto>>
{
    public async Task<Result<WeeklyLearningPlanDto>> Handle(
        GetStudentWeeklyPlanQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && !x.IsParent)
            .Select(x => new { x.Id, Name = x.User.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
            return Result<WeeklyLearningPlanDto>.NotFound("Student profile was not found.");

        var names = new Dictionary<int, string> { [student.Id] = student.Name };
        var plan = await LearningPathQueries.BuildWeeklyPlanAsync(
            dbContext, [student.Id], names, teacherId: null, cancellationToken);

        return Result<WeeklyLearningPlanDto>.Success(plan);
    }
}
