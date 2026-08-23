using Academy.Application.Features.SuperAdmin.Billing.Queries.GetAdminGroupLedger;
using Academy.Application.Features.SuperAdmin.Billing.Queries.GetAdminLessonDebts;
using Academy.Application.Features.SuperAdmin.Billing.Queries.GetAdminPaymentReceiptPdf;
using Academy.Application.Features.SuperAdmin.Billing.Queries.GetAdminStudentLessonLedger;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Authorize(Roles = AppRoles.SuperAdmin)]
[Route("api/super-admin/billing")]
[Produces("application/json")]
public sealed class BillingController(ISender sender) : ControllerBase
{
    [HttpGet("debts")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminDebtRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDebts([FromQuery] int? lessonId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminLessonDebtsQuery(lessonId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}/groups/{groupId:int}/ledger")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerStudentRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupLedger(
        int lessonId,
        int groupId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminGroupLedgerQuery(lessonId, groupId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}/students/{studentId:int}/ledger")]
    [ProducesResponseType(typeof(StudentLessonLedgerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentLedger(
        int lessonId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminStudentLessonLedgerQuery(lessonId, studentId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("payments/{paymentId:int}/receipt")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadReceipt(int paymentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminPaymentReceiptPdfQuery(paymentId), cancellationToken);
        if (!result.IsSuccess)
            return result.ToActionResult();

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }
}
