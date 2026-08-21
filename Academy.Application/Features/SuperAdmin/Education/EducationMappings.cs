using Academy.Application.Common.Localization;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;

namespace Academy.Application.Features.SuperAdmin.Education;

internal static class EducationMappings
{
    public static EducationTypeDto ToTypeDto(EducationType entity, AppLanguage language, int stagesCount)
        => new()
        {
            Id = entity.Id,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, language),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            StagesCount = stagesCount
        };

    public static EducationStageDto ToStageDto(EducationStage entity, AppLanguage language, int yearsCount)
        => new()
        {
            Id = entity.Id,
            EducationTypeId = entity.EducationTypeId,
            EducationTypeName = LocalizedNames.Pick(
                entity.EducationType.NameAr,
                entity.EducationType.NameEn,
                language),
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, language),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            YearsCount = yearsCount
        };

    public static EducationYearDto ToYearDto(EducationYear entity, AppLanguage language, int subjectsCount)
        => new()
        {
            Id = entity.Id,
            EducationStageId = entity.EducationStageId,
            EducationStageName = LocalizedNames.Pick(
                entity.EducationStage.NameAr,
                entity.EducationStage.NameEn,
                language),
            EducationTypeId = entity.EducationStage.EducationTypeId,
            EducationTypeName = LocalizedNames.Pick(
                entity.EducationStage.EducationType.NameAr,
                entity.EducationStage.EducationType.NameEn,
                language),
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, language),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            SubjectsCount = subjectsCount
        };

    public static EducationSubjectDto ToSubjectDto(EducationSubject entity, AppLanguage language)
        => new()
        {
            Id = entity.Id,
            EducationYearId = entity.EducationYearId,
            EducationYearName = LocalizedNames.Pick(
                entity.EducationYear.NameAr,
                entity.EducationYear.NameEn,
                language),
            EducationStageId = entity.EducationYear.EducationStageId,
            EducationStageName = LocalizedNames.Pick(
                entity.EducationYear.EducationStage.NameAr,
                entity.EducationYear.EducationStage.NameEn,
                language),
            EducationTypeId = entity.EducationYear.EducationStage.EducationTypeId,
            EducationTypeName = LocalizedNames.Pick(
                entity.EducationYear.EducationStage.EducationType.NameAr,
                entity.EducationYear.EducationStage.EducationType.NameEn,
                language),
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, language),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        };
}
