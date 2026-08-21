using Academy.Application.Common.Models;
using Academy.Application.Features.Auth.Dtos;
using MediatR;

namespace Academy.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponseDto>>;
