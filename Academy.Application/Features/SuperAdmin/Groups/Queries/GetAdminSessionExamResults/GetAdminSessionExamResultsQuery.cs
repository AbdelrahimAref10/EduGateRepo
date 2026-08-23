using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminSessionExamResults;

public sealed record GetAdminSessionExamResultsQuery(int SessionId)
    : IRequest<Result<TeacherExamResultsDto>>;
