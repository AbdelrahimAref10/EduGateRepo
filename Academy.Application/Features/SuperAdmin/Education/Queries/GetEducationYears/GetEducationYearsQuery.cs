using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYears;

public sealed record GetEducationYearsQuery(int EducationStageId, bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<EducationYearDto>>>;
