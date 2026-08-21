using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationSubject;

public sealed class CreateEducationSubjectCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateEducationSubjectCommand, Result<EducationSubjectDto>>
{
    public async Task<Result<EducationSubjectDto>> Handle(
        CreateEducationSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var year = await dbContext.EducationYears
            .Include(x => x.EducationStage)
                .ThenInclude(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == request.EducationYearId
                    && x.EducationStageId == request.EducationStageId
                    && x.EducationStage.EducationTypeId == request.EducationTypeId,
                cancellationToken);

        if (year is null)
            return Result<EducationSubjectDto>.NotFound("Education year was not found.");

        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.EducationSubjects
            .AnyAsync(
                x => x.EducationYearId == request.EducationYearId && x.NameEn == nameEn,
                cancellationToken);

        if (exists)
            return Result<EducationSubjectDto>.Conflict("Subject already exists for this year.");

        var entity = new EducationSubject
        {
            EducationYearId = request.EducationYearId,
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        dbContext.EducationSubjects.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        entity.EducationYear = year;
        return Result<EducationSubjectDto>.Success(
            EducationMappings.ToSubjectDto(entity, requestLanguage.Current));
    }
}
