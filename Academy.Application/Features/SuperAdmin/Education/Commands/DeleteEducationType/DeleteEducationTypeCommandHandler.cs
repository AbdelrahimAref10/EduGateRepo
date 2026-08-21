using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationType;

public sealed class DeleteEducationTypeCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteEducationTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteEducationTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationTypes
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Education type was not found.");

        if (await dbContext.EducationYears.AnyAsync(x => x.EducationTypeId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف نوع التعليم لأنه يحتوي على سنوات دراسية. احذف السنوات أولاً.");

        if (await dbContext.Lessons.AnyAsync(x => x.EducationTypeId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف نوع التعليم لأنه مرتبط بدروس.");

        dbContext.EducationTypes.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
