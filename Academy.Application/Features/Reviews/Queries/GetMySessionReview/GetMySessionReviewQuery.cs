using Academy.Application.Common.Models;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;

namespace Academy.Application.Features.Reviews.Queries.GetMySessionReview;

public sealed record GetMySessionReviewQuery(int UserId, int SessionId)
    : IRequest<Result<MyTargetReviewDto>>;
