using Academy.Application.Common.Models;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;

namespace Academy.Application.Features.Reviews.Queries.GetMyLessonReview;

public sealed record GetMyLessonReviewQuery(int UserId, int LessonId)
    : IRequest<Result<MyTargetReviewDto>>;
