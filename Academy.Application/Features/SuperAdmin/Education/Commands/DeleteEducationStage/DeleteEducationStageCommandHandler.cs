using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationStage;

public sealed class DeleteEducationStageCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteEducationStageCommand, Result>
{
    public async Task<Result> Handle(DeleteEducationStageCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationStages
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Education stage was not found.");

        if (await dbContext.EducationYears.AnyAsync(x => x.EducationStageId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف المرحلة لأنها تحتوي على صفوف. احذف الصفوف أولاً.");

        if (await dbContext.Lessons.AnyAsync(x => x.EducationStageId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف المرحلة لأنها مرتبطة بدروس.");

        dbContext.EducationStages.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
