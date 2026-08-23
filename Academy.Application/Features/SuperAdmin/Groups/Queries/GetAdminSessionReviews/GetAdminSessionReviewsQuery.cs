using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminSessionReviews;

public sealed record GetAdminSessionReviewsQuery(int SessionId)
    : IRequest<Result<AdminReviewsDto>>;
