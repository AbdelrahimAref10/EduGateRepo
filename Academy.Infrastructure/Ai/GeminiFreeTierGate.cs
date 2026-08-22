namespace Academy.Infrastructure.Ai;

/// <summary>
/// Serializes Gemini free-tier calls so RPM bursts do not look like an all-day outage.
/// </summary>
internal static class GeminiFreeTierGate
{
    private static readonly SemaphoreSlim Mutex = new(1, 1);
    private static DateTimeOffset NextAllowedUtc = DateTimeOffset.MinValue;

    public static async Task<IDisposable> EnterAsync(CancellationToken cancellationToken)
    {
        await Mutex.WaitAsync(cancellationToken);
        var wait = NextAllowedUtc - DateTimeOffset.UtcNow;
        if (wait > TimeSpan.Zero)
            await Task.Delay(wait, cancellationToken);

        return new Lease();
    }

    public static void Cooldown(TimeSpan delay)
    {
        var until = DateTimeOffset.UtcNow + delay;
        if (until > NextAllowedUtc)
            NextAllowedUtc = until;
    }

    private sealed class Lease : IDisposable
    {
        public void Dispose()
        {
            Cooldown(TimeSpan.FromSeconds(4));
            Mutex.Release();
        }
    }
}
