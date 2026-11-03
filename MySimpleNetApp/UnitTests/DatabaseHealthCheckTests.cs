using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySimpleNetApp.WebUI.Health;

namespace MySimpleNetApp.UnitTests;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsConnected_ReturnsHealthy()
    {
        var check = new DatabaseHealthCheck(new FakeDatabaseConnectionChecker(true));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsNotConnected_ReturnsUnhealthy()
    {
        var check = new DatabaseHealthCheck(new FakeDatabaseConnectionChecker(false));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
