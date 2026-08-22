using Academy.Application.Common.Models;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;

namespace Academy.Application.Features.Marketplace.Queries.GetMyTeacherReview;

public sealed record GetMyTeacherReviewQuery(int UserId, int TeacherId)
    : IRequest<Result<MyTeacherReviewDto>>;
