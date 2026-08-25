using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Students.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Students.Queries.GetLessonGroupsForTransfer;

public sealed record GetLessonGroupsForTransferQuery(int UserId, int StudentId, int LessonId)
    : IRequest<Result<IReadOnlyList<TeacherStudentGroupDto>>>;
