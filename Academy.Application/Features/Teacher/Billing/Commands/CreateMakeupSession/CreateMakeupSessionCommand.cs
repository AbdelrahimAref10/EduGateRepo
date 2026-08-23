using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Billing.Commands.CreateMakeupSession;

public sealed record CreateMakeupSessionCommand(
    int UserId,
    int LessonId,
    int GroupId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    string? Topic,
    int? MakeupForSessionId,
    IReadOnlyList<int> StudentIds,
    bool IsFree,
    decimal? Amount,
    ChargeSettlement Settlement) : IRequest<Result<LessonGroupSessionDto>>;
