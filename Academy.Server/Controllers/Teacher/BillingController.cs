using System.Security.Claims;
using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Billing.Commands.CreateMakeupSession;
using Academy.Application.Features.Teacher.Billing.Commands.RecordPayment;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Application.Features.Teacher.Billing.Queries.GetGroupLedger;
using Academy.Application.Features.Teacher.Billing.Queries.GetLessonDebts;
using Academy.Application.Features.Teacher.Billing.Queries.GetPaymentReceiptPdf;
using Academy.Application.Features.Teacher.Billing.Queries.GetStudentLessonLedger;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingCatalog;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingLessons;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherLessonBillingDetail;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Teacher;

[ApiController]
[Authorize(Roles = AppRoles.Teacher)]
[Route("api/teacher/billing")]
[Produces("application/json")]
public sealed class BillingController(ISender sender) : ControllerBase
{
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(BillingCatalogDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingCatalog(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingCatalogQuery(userId.Value),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("lessons")]
    [ProducesResponseType(typeof(PagedResult<BillingLessonSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingLessons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingLessonsQuery(userId.Value, page, pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}")]
    [ProducesResponseType(typeof(LessonBillingDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonBillingDetail(
        int lessonId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherLessonBillingDetailQuery(userId.Value, lessonId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}/groups/{groupId:int}/ledger")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerStudentRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupLedger(
        int lessonId,
        int groupId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetGroupLedgerQuery(userId.Value, lessonId, groupId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}/students/{studentId:int}/ledger")]
    [ProducesResponseType(typeof(StudentLessonLedgerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentLedger(
        int lessonId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetStudentLessonLedgerQuery(userId.Value, lessonId, studentId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}/debts")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerStudentRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDebts(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetLessonDebtsQuery(userId.Value, lessonId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("lessons/{lessonId:int}/payments")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordPayment(
        int lessonId,
        [FromBody] RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new RecordPaymentCommand(
                userId.Value,
                lessonId,
                request.StudentId,
                request.Amount,
                request.Method,
                request.Note,
                request.ChargeIds,
                request.PaidAtUtc),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("payments/{paymentId:int}/receipt")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadReceipt(int paymentId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetPaymentReceiptPdfQuery(userId.Value, paymentId, AsStudent: false),
            cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }

    [HttpPost("lessons/{lessonId:int}/groups/{groupId:int}/makeup-sessions")]
    [ProducesResponseType(typeof(LessonGroupSessionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateMakeup(
        int lessonId,
        int groupId,
        [FromBody] CreateMakeupSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new CreateMakeupSessionCommand(
                userId.Value,
                lessonId,
                groupId,
                request.SessionDate,
                request.StartTime,
                request.Topic,
                request.MakeupForSessionId,
                request.StudentIds,
                request.IsFree,
                request.Amount,
                request.Settlement),
            cancellationToken);

        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }
}
