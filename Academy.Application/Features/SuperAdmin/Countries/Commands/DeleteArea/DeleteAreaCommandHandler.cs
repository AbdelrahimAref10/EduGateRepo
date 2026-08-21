using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteArea;

public sealed class DeleteAreaCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteAreaCommand, Result>
{
    public async Task<Result> Handle(DeleteAreaCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Areas
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Area was not found.");

        if (await dbContext.Areas.AnyAsync(x => x.Id == request.Id && x.Users.Any(), cancellationToken))
            return Result.Conflict("لا يمكن حذف المنطقة لأنها مرتبطة بمستخدمين.");

        if (await dbContext.Lessons.AnyAsync(x => x.AreaId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف المنطقة لأنها مرتبطة بدروس.");

        if (await dbContext.LessonGroups.AnyAsync(x => x.AreaId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف المنطقة لأنها مرتبطة بمجموعات دروس.");

        dbContext.Areas.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
