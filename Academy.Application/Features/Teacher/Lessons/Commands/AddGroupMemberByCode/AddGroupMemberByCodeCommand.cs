using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.AddGroupMemberByCode;

public sealed record AddGroupMemberByCodeCommand(
    int UserId,
    int LessonId,
    int GroupId,
    int? StudentId,
    string? StudentCode) : IRequest<Result<LessonGroupDto>>;
