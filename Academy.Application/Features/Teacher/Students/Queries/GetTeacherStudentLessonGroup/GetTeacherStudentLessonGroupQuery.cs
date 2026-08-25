using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Students.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Students.Queries.GetTeacherStudentLessonGroup;

public sealed record GetTeacherStudentLessonGroupQuery(int UserId, int StudentId, int LessonId)
    : IRequest<Result<TeacherStudentGroupDto>>;
