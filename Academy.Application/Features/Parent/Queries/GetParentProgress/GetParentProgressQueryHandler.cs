using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.LearningPath.Common;
using Academy.Application.Features.LearningPath.Dtos;
using Academy.Application.Features.Parent.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetParentProgress;

public sealed record GetParentProgressQuery(int UserId, int? ChildStudentId)
    : IRequest<Result<ProgressReportDto>>;

public sealed class GetParentProgressQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetParentProgressQuery, Result<ProgressReportDto>>
{
    public async Task<Result<ProgressReportDto>> Handle(
        GetParentProgressQuery request,
        CancellationToken cancellationToken)
    {
        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<ProgressReportDto>.NotFound("Parent profile was not found.");

        var linkedIds = await ParentAccess.GetLinkedChildStudentIdsAsync(
            dbContext, parentStudentId.Value, cancellationToken);

        IReadOnlyList<int> childIds = linkedIds;
        if (request.ChildStudentId is > 0)
        {
            if (!linkedIds.Contains(request.ChildStudentId.Value))
                return Result<ProgressReportDto>.Failure("This child is not linked to your account.", 403);

            childIds = [request.ChildStudentId.Value];
        }

        var names = await dbContext.Students
            .AsNoTracking()
            .Where(s => childIds.Contains(s.Id))
            .Select(s => new { s.Id, Name = s.User.FullName })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var lessons = await LearningPathQueries.BuildProgressAsync(
            dbContext, childIds, names, teacherId: null, lessonId: null, cancellationToken);

        return Result<ProgressReportDto>.Success(new ProgressReportDto { Lessons = lessons });
    }
}
