using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MySimpleNetApp.WebUI.Data;

public static class DatabaseInitializer
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Czeka, aż baza przyjmie połączenie, i tworzy schemat (EnsureCreated).
    /// W compose baza ma healthcheck, ale przy `docker run` samej aplikacji i w Azure
    /// start aplikacji może wyprzedzić bazę.
    /// </summary>
    public static async Task InitializeAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                // Nowy scope na każdą próbę: DbContext po nieudanym połączeniu nie nadaje się do użycia.
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await db.Database.EnsureCreatedAsync(cancellationToken);
                logger.LogInformation("Database schema is ready (attempt {Attempt}).", attempt);
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts && (ex is NpgsqlException or TimeoutException))
            {
                logger.LogWarning(
                    "Database is not ready yet (attempt {Attempt}/{MaxAttempts}): {Message}",
                    attempt, MaxAttempts, ex.Message);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }
}
