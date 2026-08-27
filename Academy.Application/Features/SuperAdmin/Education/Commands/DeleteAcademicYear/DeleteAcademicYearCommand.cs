using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteAcademicYear;

public sealed record DeleteAcademicYearCommand(int Id) : IRequest<Result>;
