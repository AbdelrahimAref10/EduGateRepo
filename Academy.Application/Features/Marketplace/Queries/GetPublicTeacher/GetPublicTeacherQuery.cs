using Academy.Application.Common.Models;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicTeacher;

public sealed record GetPublicTeacherQuery(int TeacherId, int? ViewerUserId)
    : IRequest<Result<PublicTeacherDetailDto>>;
