using MySimpleNetApp.WebUI.Data;

namespace MySimpleNetApp.UnitTests;

public sealed class FakeDatabaseConnectionChecker(bool connected) : IDatabaseConnectionChecker
{
    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(connected);
}
