using Academy.Application.Common.Models;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;

namespace Academy.Application.Features.Reviews.Commands.UpsertSessionReview;

public sealed record UpsertSessionReviewCommand(int UserId, int SessionId, int Rating, string? Comment)
    : IRequest<Result<TargetReviewDto>>;
