using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.Teacher.Students.Commands.TransferStudentGroup;

public sealed record TransferStudentGroupCommand(
    int UserId,
    int StudentId,
    int LessonId,
    int TargetGroupId) : IRequest<Result<int>>;
