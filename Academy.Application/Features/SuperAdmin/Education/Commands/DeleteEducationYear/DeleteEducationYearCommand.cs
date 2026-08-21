using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationYear;

public sealed record DeleteEducationYearCommand(int EducationTypeId, int YearId) : IRequest<Result>;
