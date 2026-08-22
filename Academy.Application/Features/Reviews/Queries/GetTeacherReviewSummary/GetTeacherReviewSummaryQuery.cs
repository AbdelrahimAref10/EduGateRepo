using Academy.Application.Common.Models;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;

namespace Academy.Application.Features.Reviews.Queries.GetTeacherReviewSummary;

public sealed record GetTeacherReviewSummaryQuery(int UserId)
    : IRequest<Result<TeacherReviewSummaryDto>>;
