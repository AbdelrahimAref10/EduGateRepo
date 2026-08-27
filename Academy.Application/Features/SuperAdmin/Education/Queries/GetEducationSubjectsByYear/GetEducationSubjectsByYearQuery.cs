using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationSubjectsByYear;

public sealed record GetEducationSubjectsByYearQuery(
    int EducationStageId,
    int EducationYearId,
    bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<EducationSubjectDto>>>;
