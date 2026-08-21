using Academy.Application.Common.Models;
using Academy.Application.Features.Auth.Dtos;
using MediatR;

namespace Academy.Application.Features.Account.Commands.UpdatePreferredLanguage;

public sealed record UpdatePreferredLanguageCommand(int UserId, int LanguageId)
    : IRequest<Result<AuthResponseDto>>;
