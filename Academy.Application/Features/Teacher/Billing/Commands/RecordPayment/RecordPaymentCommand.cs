using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Billing.Commands.RecordPayment;

public sealed record RecordPaymentCommand(
    int UserId,
    int LessonId,
    int StudentId,
    decimal Amount,
    PaymentMethod Method,
    string? Note,
    IReadOnlyList<int>? ChargeIds,
    DateTime? PaidAtUtc) : IRequest<Result<PaymentDto>>;
