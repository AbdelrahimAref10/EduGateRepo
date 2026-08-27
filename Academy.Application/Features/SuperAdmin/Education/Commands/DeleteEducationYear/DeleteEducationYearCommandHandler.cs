using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationYear;

public sealed class DeleteEducationYearCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteEducationYearCommand, Result>
{
    public async Task<Result> Handle(DeleteEducationYearCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationYears
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.YearId && x.EducationStageId == request.EducationStageId,
                cancellationToken);

        if (entity is null)
            return Result.NotFound("Education year was not found.");

        if (await dbContext.EducationSubjects.AnyAsync(x => x.EducationYearId == request.YearId, cancellationToken))
            return Result.Conflict("لا يمكن حذف الصف لأنه يحتوي على مواد. احذف المواد أولاً.");

        if (await dbContext.Lessons.AnyAsync(x => x.EducationYearId == request.YearId, cancellationToken))
            return Result.Conflict("لا يمكن حذف الصف لأنه مرتبط بدروس.");

        dbContext.EducationYears.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
