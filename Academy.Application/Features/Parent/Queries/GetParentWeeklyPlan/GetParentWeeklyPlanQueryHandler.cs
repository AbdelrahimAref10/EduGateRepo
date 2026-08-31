using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.LearningPath.Common;
using Academy.Application.Features.LearningPath.Dtos;
using Academy.Application.Features.Parent.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetParentWeeklyPlan;

public sealed record GetParentWeeklyPlanQuery(int UserId, int? ChildStudentId)
    : IRequest<Result<WeeklyLearningPlanDto>>;

public sealed class GetParentWeeklyPlanQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetParentWeeklyPlanQuery, Result<WeeklyLearningPlanDto>>
{
    public async Task<Result<WeeklyLearningPlanDto>> Handle(
        GetParentWeeklyPlanQuery request,
        CancellationToken cancellationToken)
    {
        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<WeeklyLearningPlanDto>.NotFound("Parent profile was not found.");

        var linkedIds = await ParentAccess.GetLinkedChildStudentIdsAsync(
            dbContext, parentStudentId.Value, cancellationToken);

        IReadOnlyList<int> childIds = linkedIds;
        if (request.ChildStudentId is > 0)
        {
            if (!linkedIds.Contains(request.ChildStudentId.Value))
                return Result<WeeklyLearningPlanDto>.Failure("This child is not linked to your account.", 403);

            childIds = [request.ChildStudentId.Value];
        }

        var names = await dbContext.Students
            .AsNoTracking()
            .Where(s => childIds.Contains(s.Id))
            .Select(s => new { s.Id, Name = s.User.FullName })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var plan = await LearningPathQueries.BuildWeeklyPlanAsync(
            dbContext, childIds, names, teacherId: null, cancellationToken);

        return Result<WeeklyLearningPlanDto>.Success(plan);
    }
}
