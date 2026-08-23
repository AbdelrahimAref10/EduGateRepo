using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetLessonDebts;

public sealed record GetLessonDebtsQuery(
    int UserId,
    int LessonId) : IRequest<Result<IReadOnlyList<LedgerStudentRowDto>>>;
