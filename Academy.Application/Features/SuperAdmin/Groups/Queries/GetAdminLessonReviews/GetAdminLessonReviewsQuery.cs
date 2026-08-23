using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminLessonReviews;

public sealed record GetAdminLessonReviewsQuery(int LessonId)
    : IRequest<Result<AdminReviewsDto>>;
