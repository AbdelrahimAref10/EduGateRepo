using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Lessons.Queries.GetStudentLessonDetail;

public sealed record GetStudentLessonDetailQuery(int UserId, int LessonId)
    : IRequest<Result<StudentLessonDetailDto>>;
