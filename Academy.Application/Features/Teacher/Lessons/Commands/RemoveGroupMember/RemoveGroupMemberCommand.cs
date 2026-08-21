using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.RemoveGroupMember;

public sealed record RemoveGroupMemberCommand(
    int UserId,
    int LessonId,
    int GroupId,
    int StudentId) : IRequest<Result>;
