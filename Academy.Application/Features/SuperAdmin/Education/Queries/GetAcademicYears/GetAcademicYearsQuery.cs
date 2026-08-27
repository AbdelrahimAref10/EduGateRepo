using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetAcademicYears;

public sealed record GetAcademicYearsQuery(bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<AcademicYearDto>>>;
