using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetGroupLedger;

public sealed record GetGroupLedgerQuery(
    int UserId,
    int LessonId,
    int GroupId) : IRequest<Result<IReadOnlyList<LedgerStudentRowDto>>>;
