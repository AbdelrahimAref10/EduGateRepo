using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateAcademicYear;

public sealed record UpdateAcademicYearCommand(int Id, string Name, int SortOrder)
    : IRequest<Result<AcademicYearDto>>;
