using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherSessionExam;

public sealed record GetTeacherSessionExamQuery(int UserId, int SessionId)
    : IRequest<Result<TeacherExamDto?>>;
