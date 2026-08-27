using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationSubject;

public sealed record UpdateEducationSubjectCommand(
    int EducationStageId,
    int EducationYearId,
    int SubjectId,
    string NameAr,
    string NameEn,
    int SortOrder) : IRequest<Result<EducationSubjectDto>>;
