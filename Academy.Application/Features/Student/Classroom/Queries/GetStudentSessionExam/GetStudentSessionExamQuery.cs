using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;

public sealed record GetStudentSessionExamQuery(int UserId, int SessionId)
    : IRequest<Result<StudentExamDto?>>;
