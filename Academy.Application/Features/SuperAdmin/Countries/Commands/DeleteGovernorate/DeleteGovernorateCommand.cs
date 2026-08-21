using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteGovernorate;

public sealed record DeleteGovernorateCommand(int Id) : IRequest<Result>;
