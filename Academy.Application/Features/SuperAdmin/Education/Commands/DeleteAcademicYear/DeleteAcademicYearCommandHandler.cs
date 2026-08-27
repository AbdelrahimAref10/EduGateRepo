using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteAcademicYear;

public sealed class DeleteAcademicYearCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteAcademicYearCommand, Result>
{
    public async Task<Result> Handle(DeleteAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.AcademicYears
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Academic year was not found.");

        if (await dbContext.Lessons.AnyAsync(x => x.AcademicYearId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف السنة الدراسية لأنها مرتبطة بدروس.");

        dbContext.AcademicYears.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
