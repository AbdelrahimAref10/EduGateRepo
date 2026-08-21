using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYearsByStage;

public sealed record GetEducationYearsByStageQuery(
    int EducationTypeId,
    int EducationStageId,
    bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<EducationYearDto>>>;
