using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationSubject;

public sealed class DeleteEducationSubjectCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteEducationSubjectCommand, Result>
{
    public async Task<Result> Handle(DeleteEducationSubjectCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationSubjects
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.SubjectId
                    && x.EducationYearId == request.EducationYearId
                    && x.EducationYear.EducationStageId == request.EducationStageId,
                cancellationToken);

        if (entity is null)
            return Result.NotFound("Subject was not found.");

        if (await dbContext.Lessons.AnyAsync(x => x.EducationSubjectId == request.SubjectId, cancellationToken))
            return Result.Conflict("لا يمكن حذف المادة لأنها مرتبطة بدروس.");

        dbContext.EducationSubjects.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
