using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYearsByType;

public sealed record GetEducationYearsByTypeQuery(int EducationTypeId, bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<EducationYearDto>>>;
