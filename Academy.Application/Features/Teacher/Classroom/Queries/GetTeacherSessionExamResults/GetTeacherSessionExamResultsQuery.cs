using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherSessionExamResults;

public sealed record GetTeacherSessionExamResultsQuery(int UserId, int SessionId)
    : IRequest<Result<TeacherExamResultsDto>>;
