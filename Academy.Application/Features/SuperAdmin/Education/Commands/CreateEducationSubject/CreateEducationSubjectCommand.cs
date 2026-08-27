using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationSubject;

public sealed record CreateEducationSubjectCommand(
    int EducationStageId,
    int EducationYearId,
    string NameAr,
    string NameEn,
    int SortOrder) : IRequest<Result<EducationSubjectDto>>;
