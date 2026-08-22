using Academy.Application.Common.Models;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;

namespace Academy.Application.Features.Reviews.Commands.UpsertLessonReview;

public sealed record UpsertLessonReviewCommand(int UserId, int LessonId, int Rating, string? Comment)
    : IRequest<Result<TargetReviewDto>>;
