using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Billing.Common;

internal static class LedgerChargeStatus
{
    public static string Resolve(ChargeStatus status, decimal amount, decimal allocated)
    {
        if (status == ChargeStatus.Deferred)
            return nameof(ChargeStatus.Deferred);

        var remaining = amount - allocated;
        if (remaining <= 0)
            return nameof(ChargeStatus.Paid);

        if (allocated > 0)
            return nameof(ChargeStatus.Partial);

        return nameof(ChargeStatus.Open);
    }
}
