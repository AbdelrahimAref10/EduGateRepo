using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteCity;

public sealed record DeleteCityCommand(int Id) : IRequest<Result>;
