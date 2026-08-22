using Academy.Application.Common.Models;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicTeachers;

public sealed record GetPublicTeachersQuery(
    int? CountryId,
    int? EducationStageId,
    int? EducationSubjectId)
    : IRequest<Result<IReadOnlyList<PublicTeacherListItemDto>>>;
