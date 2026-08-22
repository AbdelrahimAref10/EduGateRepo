using Academy.Application.Common.Models;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;

namespace Academy.Application.Features.Marketplace.Commands.UpdateTeacherReview;

public sealed record UpdateTeacherReviewCommand(
    int UserId,
    int TeacherId,
    int Rating,
    string? Comment)
    : IRequest<Result<TeacherReviewDto>>;
