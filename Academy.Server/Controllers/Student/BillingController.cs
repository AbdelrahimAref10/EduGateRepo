using System.Security.Claims;
using Academy.Application.Features.Student.Billing.Queries.GetMyLessonPayments;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Application.Features.Teacher.Billing.Queries.GetPaymentReceiptPdf;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Student;

[ApiController]
[Authorize(Roles = AppRoles.Student)]
[Route("api/student/billing")]
[Produces("application/json")]
public sealed class BillingController(ISender sender) : ControllerBase
{
    [HttpGet("lessons/{lessonId:int}/payments")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayments(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetMyLessonPaymentsQuery(userId.Value, lessonId),
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
            new GetPaymentReceiptPdfQuery(userId.Value, paymentId, AsStudent: true),
            cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        return File(result.Value!.Content, result.Value.ContentType, result.Value.FileName);
    }

    private int? GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }
}
