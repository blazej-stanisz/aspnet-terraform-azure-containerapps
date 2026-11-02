using MySimpleNetApp.WebUI.Monitoring;

namespace MySimpleNetApp.UnitTests;

public sealed class FakeMetricsSnapshotProvider(MetricsSnapshot snapshot) : IMetricsSnapshotProvider
{
    public Task<MetricsSnapshot> GetAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(snapshot);
}
