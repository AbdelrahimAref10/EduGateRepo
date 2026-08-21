using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons;

internal static class EducationCurriculum
{
    public static async Task<Result<EducationSubject>> ResolveSubjectAsync(
        IApplicationDbContext dbContext,
        int educationTypeId,
        int educationStageId,
        int educationYearId,
        int educationSubjectId,
        CancellationToken cancellationToken)
    {
        var subject = await dbContext.EducationSubjects
            .Include(x => x.EducationYear)
                .ThenInclude(x => x.EducationStage)
                    .ThenInclude(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == educationSubjectId
                     && x.IsActive
                     && x.EducationYear.IsActive
                     && x.EducationYear.EducationStage.IsActive
                     && x.EducationYear.EducationStage.EducationType.IsActive,
                cancellationToken);

        if (subject is null)
            return Result<EducationSubject>.NotFound("Subject was not found.");

        if (subject.EducationYearId != educationYearId)
            return Result<EducationSubject>.Failure("Subject does not belong to the selected education year.");

        if (subject.EducationYear.EducationStageId != educationStageId)
            return Result<EducationSubject>.Failure("Education year does not belong to the selected stage.");

        if (subject.EducationYear.EducationStage.EducationTypeId != educationTypeId)
            return Result<EducationSubject>.Failure("Education stage does not belong to the selected type.");

        return Result<EducationSubject>.Success(subject);
    }
}
