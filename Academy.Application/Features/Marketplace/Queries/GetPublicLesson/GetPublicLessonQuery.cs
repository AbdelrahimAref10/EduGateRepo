using Academy.Application.Common.Models;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicLesson;

public sealed record GetPublicLessonQuery(int LessonId)
    : IRequest<Result<PublicLessonDeepLinkDto>>;
