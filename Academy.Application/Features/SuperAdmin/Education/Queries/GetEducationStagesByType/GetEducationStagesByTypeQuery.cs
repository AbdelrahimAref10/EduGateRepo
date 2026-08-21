using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationStagesByType;

public sealed record GetEducationStagesByTypeQuery(int EducationTypeId, bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<EducationStageDto>>>;
