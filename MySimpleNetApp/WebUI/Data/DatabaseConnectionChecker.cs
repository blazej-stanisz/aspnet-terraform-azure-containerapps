using Microsoft.EntityFrameworkCore;

namespace MySimpleNetApp.WebUI.Data;

public sealed class DatabaseConnectionChecker(
    AppDbContext db,
    ILogger<DatabaseConnectionChecker> logger) : IDatabaseConnectionChecker
{
    // Bez limitu strona główna czekałaby ~15 s, gdy baza nie odpowiada.
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(Timeout);

        try
        {
            var connected = await db.Database.CanConnectAsync(timeoutSource.Token);
            if (!connected)
            {
                logger.LogWarning("Database connection check failed within {Timeout}.", Timeout);
            }

            return connected;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Database connection check timed out after {Timeout}.", Timeout);
            return false;
        }
    }
}
