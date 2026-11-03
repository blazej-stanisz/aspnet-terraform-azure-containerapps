using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySimpleNetApp.WebUI.Data;

namespace MySimpleNetApp.WebUI.Health;

public sealed class DatabaseHealthCheck(IDatabaseConnectionChecker database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connected = await database.IsConnectedAsync(cancellationToken);

        return connected
            ? HealthCheckResult.Healthy("PostgreSQL is reachable.")
            : HealthCheckResult.Unhealthy("PostgreSQL is not reachable.");
    }
}
