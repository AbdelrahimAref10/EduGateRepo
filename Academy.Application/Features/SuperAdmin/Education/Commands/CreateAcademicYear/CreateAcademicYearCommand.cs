using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateAcademicYear;

public sealed record CreateAcademicYearCommand(string Name, int SortOrder)
    : IRequest<Result<AcademicYearDto>>;
