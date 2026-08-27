using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationStages;

public sealed record GetEducationStagesQuery(bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<EducationStageDto>>>;
