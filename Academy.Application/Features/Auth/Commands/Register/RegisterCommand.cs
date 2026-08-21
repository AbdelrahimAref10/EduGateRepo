using Academy.Application.Common.Models;
using Academy.Application.Features.Auth.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    AppRole Role,
    int AreaId) : IRequest<Result<AuthResponseDto>>;
