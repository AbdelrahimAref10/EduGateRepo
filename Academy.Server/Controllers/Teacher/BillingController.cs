using System.Security.Claims;
using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Billing.Commands.CreateMakeupSession;
using Academy.Application.Features.Teacher.Billing.Commands.RecordPayment;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Application.Features.Teacher.Billing.Queries.GetPaymentReceiptPdf;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingCharge;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingCharges;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterAcademicYears;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterGroups;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterLessons;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterSessions;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterStages;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingOutstanding;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingPayment;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingPayments;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingStudents;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingSummary;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingTransactions;
using Academy.Application.Features.Teacher.Billing.Queries.GetTeacherStudentOutstanding;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Common;
using Academy.Domain.Enums;
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
    [HttpGet("summary")]
    [ProducesResponseType(typeof(TeacherBillingSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int? studentId,
        [FromQuery] int? academicYearId,
        [FromQuery] int? educationStageId,
        [FromQuery] int? lessonId,
        [FromQuery] int? groupId,
        [FromQuery] int? sessionId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingSummaryQuery(
                userId.Value,
                studentId,
                academicYearId,
                educationStageId,
                lessonId,
                groupId,
                sessionId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(PagedResult<LedgerTransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int? studentId,
        [FromQuery] int? academicYearId,
        [FromQuery] int? educationStageId,
        [FromQuery] int? lessonId,
        [FromQuery] int? groupId,
        [FromQuery] int? sessionId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] ChargeType? type,
        [FromQuery] string? kind,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = LedgerPaging.PageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingTransactionsQuery(
                userId.Value,
                studentId,
                academicYearId,
                educationStageId,
                lessonId,
                groupId,
                sessionId,
                from,
                to,
                type,
                kind,
                page,
                pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("outstanding")]
    [ProducesResponseType(typeof(PagedResult<LedgerChargeRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutstanding(
        [FromQuery] int? studentId,
        [FromQuery] int? academicYearId,
        [FromQuery] int? educationStageId,
        [FromQuery] int? lessonId,
        [FromQuery] int? groupId,
        [FromQuery] int? sessionId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] ChargeType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = LedgerPaging.PageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingOutstandingQuery(
                userId.Value,
                studentId,
                academicYearId,
                educationStageId,
                lessonId,
                groupId,
                sessionId,
                from,
                to,
                type,
                page,
                pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("charges")]
    [ProducesResponseType(typeof(PagedResult<LedgerChargeRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCharges(
        [FromQuery] int? studentId,
        [FromQuery] int? academicYearId,
        [FromQuery] int? educationStageId,
        [FromQuery] int? lessonId,
        [FromQuery] int? groupId,
        [FromQuery] int? sessionId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] ChargeType? type,
        [FromQuery] ChargeStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = LedgerPaging.PageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingChargesQuery(
                userId.Value,
                studentId,
                academicYearId,
                educationStageId,
                lessonId,
                groupId,
                sessionId,
                from,
                to,
                type,
                status,
                page,
                pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("charges/{chargeId:int}")]
    [ProducesResponseType(typeof(LedgerChargeDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCharge(int chargeId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingChargeQuery(userId.Value, chargeId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("payments")]
    [ProducesResponseType(typeof(PagedResult<LedgerPaymentRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int? studentId,
        [FromQuery] int? academicYearId,
        [FromQuery] int? educationStageId,
        [FromQuery] int? lessonId,
        [FromQuery] int? groupId,
        [FromQuery] int? sessionId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = LedgerPaging.PageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingPaymentsQuery(
                userId.Value,
                studentId,
                academicYearId,
                educationStageId,
                lessonId,
                groupId,
                sessionId,
                from,
                to,
                page,
                pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("payments/{paymentId:int}")]
    [ProducesResponseType(typeof(LedgerPaymentDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayment(int paymentId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingPaymentQuery(userId.Value, paymentId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("payments")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordPayment(
        [FromBody] RecordTeacherPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new RecordPaymentCommand(
                userId.Value,
                request.LessonId,
                request.StudentId,
                request.Amount,
                request.Method,
                request.Note,
                request.ChargeIds,
                request.PaidAtUtc),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("students")]
    [ProducesResponseType(typeof(IReadOnlyList<BillingStudentSearchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchStudents(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingStudentsQuery(userId.Value, search),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("students/{studentId:int}/outstanding")]
    [ProducesResponseType(typeof(StudentOutstandingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentOutstanding(int studentId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherStudentOutstandingQuery(userId.Value, studentId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("filters/academic-years")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerFilterOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterAcademicYears(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingFilterAcademicYearsQuery(userId.Value),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("filters/stages")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerFilterOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterStages(
        [FromQuery] int academicYearId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingFilterStagesQuery(userId.Value, academicYearId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("filters/lessons")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerFilterOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterLessons(
        [FromQuery] int academicYearId,
        [FromQuery] int educationStageId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingFilterLessonsQuery(userId.Value, academicYearId, educationStageId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("filters/groups")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerFilterOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterGroups(
        [FromQuery] int lessonId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingFilterGroupsQuery(userId.Value, lessonId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("filters/sessions")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerFilterSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterSessions(
        [FromQuery] int groupId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherBillingFilterSessionsQuery(userId.Value, groupId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("lessons/{lessonId:int}/payments")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordLessonPayment(
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
