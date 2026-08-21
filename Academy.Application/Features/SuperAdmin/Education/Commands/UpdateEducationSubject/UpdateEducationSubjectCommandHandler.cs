using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationSubject;

public sealed class UpdateEducationSubjectCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateEducationSubjectCommand, Result<EducationSubjectDto>>
{
    public async Task<Result<EducationSubjectDto>> Handle(
        UpdateEducationSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationSubjects
            .AsTracking()
            .Include(x => x.EducationYear)
                .ThenInclude(x => x.EducationStage)
                    .ThenInclude(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == request.SubjectId
                    && x.EducationYearId == request.EducationYearId
                    && x.EducationYear.EducationStageId == request.EducationStageId
                    && x.EducationYear.EducationStage.EducationTypeId == request.EducationTypeId,
                cancellationToken);

        if (entity is null)
            return Result<EducationSubjectDto>.NotFound("Subject was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.EducationSubjects
            .AnyAsync(
                x => x.Id != request.SubjectId
                    && x.EducationYearId == request.EducationYearId
                    && x.NameEn == nameEn,
                cancellationToken);

        if (nameTaken)
            return Result<EducationSubjectDto>.Conflict("Subject already exists for this year.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;
        entity.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EducationSubjectDto>.Success(
            EducationMappings.ToSubjectDto(entity, requestLanguage.Current));
    }
}
