using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetStudentLessonLedger;

public sealed record GetStudentLessonLedgerQuery(
    int UserId,
    int LessonId,
    int StudentId) : IRequest<Result<StudentLessonLedgerDto>>;
