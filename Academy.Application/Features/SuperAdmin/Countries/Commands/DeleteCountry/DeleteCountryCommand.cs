using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteCountry;

public sealed record DeleteCountryCommand(int Id) : IRequest<Result>;
