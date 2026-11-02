using Prometheus;

namespace MySimpleNetApp.WebUI.Monitoring;

public interface IMetricsSnapshotProvider
{
    Task<MetricsSnapshot> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class PrometheusMetricsSnapshotProvider : IMetricsSnapshotProvider
{
    public async Task<MetricsSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(buffer, cancellationToken);

        var text = System.Text.Encoding.UTF8.GetString(buffer.ToArray());
        return PrometheusTextParser.Parse(text);
    }
}
