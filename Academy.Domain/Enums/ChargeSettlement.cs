namespace Academy.Domain.Enums;

/// <summary>
/// Where a paid makeup charge should land on the student's account (Monthly lessons only).
/// Per-session makeups always use <see cref="Standalone"/>.
/// </summary>
public enum ChargeSettlement
{
    /// <summary>No charge (free makeup).</summary>
    None = 0,

    /// <summary>
    /// Independent open debt — paid separately, not tied to a monthly cycle.
    /// </summary>
    Standalone = 1,

    /// <summary>Linked to the student's current monthly cycle (if one covers the date).</summary>
    CurrentCycle = 2,

    /// <summary>Deferred until the next monthly cycle is created, then activated.</summary>
    NextCycle = 3
}
