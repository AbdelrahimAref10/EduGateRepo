using Academy.Application.Common.Models;
using Academy.Application.Features.Reviews.Dtos;
using MediatR;

namespace Academy.Application.Features.Reviews.Queries.GetTeacherReviewInbox;

public sealed record GetTeacherReviewInboxQuery(int UserId, ReviewInboxKind Kind, int Skip, int Take)
    : IRequest<Result<TeacherReviewInboxDto>>;
