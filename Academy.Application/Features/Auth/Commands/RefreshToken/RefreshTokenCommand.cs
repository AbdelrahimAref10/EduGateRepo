using Academy.Application.Common.Models;
using Academy.Application.Features.Auth.Dtos;
using MediatR;

namespace Academy.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<Result<AuthResponseDto>>;
