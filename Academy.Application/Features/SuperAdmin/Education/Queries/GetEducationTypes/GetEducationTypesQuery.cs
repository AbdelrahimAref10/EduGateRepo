using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationTypes;

public sealed record GetEducationTypesQuery(bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<EducationTypeDto>>>;
