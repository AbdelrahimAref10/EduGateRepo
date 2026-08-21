using Academy.Application.Common.Models;
using Academy.Application.Features.Account.Dtos;
using MediatR;

namespace Academy.Application.Features.Account.Queries.GetMyProfile;

public sealed record GetMyProfileQuery(int UserId) : IRequest<Result<UserProfileDto>>;
