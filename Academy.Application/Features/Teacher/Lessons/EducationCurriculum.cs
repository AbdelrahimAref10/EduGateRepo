using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons;

internal static class EducationCurriculum
{
    public static async Task<Result<AcademicYear>> ResolveAcademicYearAsync(
        IApplicationDbContext dbContext,
        int academicYearId,
        CancellationToken cancellationToken)
    {
        var academicYear = await dbContext.AcademicYears
            .FirstOrDefaultAsync(x => x.Id == academicYearId && x.IsActive, cancellationToken);

        if (academicYear is null)
            return Result<AcademicYear>.NotFound("Academic year was not found.");

        return Result<AcademicYear>.Success(academicYear);
    }

    public static async Task<Result<EducationSubject>> ResolveSubjectAsync(
        IApplicationDbContext dbContext,
        int educationStageId,
        int educationYearId,
        int educationSubjectId,
        CancellationToken cancellationToken)
    {
        var subject = await dbContext.EducationSubjects
            .Include(x => x.EducationYear)
                .ThenInclude(x => x.EducationStage)
            .FirstOrDefaultAsync(
                x => x.Id == educationSubjectId
                     && x.IsActive
                     && x.EducationYear.IsActive
                     && x.EducationYear.EducationStage.IsActive,
                cancellationToken);

        if (subject is null)
            return Result<EducationSubject>.NotFound("Subject was not found.");

        if (subject.EducationYearId != educationYearId)
            return Result<EducationSubject>.Failure("Subject does not belong to the selected education year.");

        if (subject.EducationYear.EducationStageId != educationStageId)
            return Result<EducationSubject>.Failure("Education year does not belong to the selected stage.");

        return Result<EducationSubject>.Success(subject);
    }
}
