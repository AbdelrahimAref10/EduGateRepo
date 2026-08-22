using Academy.Application.Common.Models;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;

namespace Academy.Application.Features.Marketplace.Commands.CreateTeacherReview;

public sealed record CreateTeacherReviewCommand(
    int UserId,
    int TeacherId,
    int Rating,
    string? Comment)
    : IRequest<Result<TeacherReviewDto>>;
