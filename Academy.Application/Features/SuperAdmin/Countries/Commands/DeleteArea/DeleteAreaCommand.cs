using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteArea;

public sealed record DeleteAreaCommand(int Id) : IRequest<Result>;
