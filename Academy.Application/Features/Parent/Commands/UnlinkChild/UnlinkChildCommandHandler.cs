using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Commands.UnlinkChild;

public sealed record UnlinkChildCommand(int UserId, int ChildStudentId)
    : IRequest<Result>;

public sealed class UnlinkChildCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UnlinkChildCommand, Result>
{
    public async Task<Result> Handle(UnlinkChildCommand request, CancellationToken cancellationToken)
    {
        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result.NotFound("Parent profile was not found.");

        var link = await dbContext.ParentChildLinks
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.ParentStudentId == parentStudentId.Value
                     && x.ChildStudentId == request.ChildStudentId,
                cancellationToken);

        if (link is null)
            return Result.NotFound("Linked child was not found.");

        dbContext.ParentChildLinks.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
