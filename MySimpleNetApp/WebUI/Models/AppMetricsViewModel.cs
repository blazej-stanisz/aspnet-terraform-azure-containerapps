using MySimpleNetApp.WebUI.Monitoring;

namespace MySimpleNetApp.WebUI.Models;

public sealed record MetricTile(string Label, string Value, string? Hint = null);

public sealed record MetricTable(
    string Title,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows);

public sealed class AppMetricsViewModel
{
    public required DateTimeOffset GeneratedAt { get; init; }

    public required IReadOnlyList<MetricTile> Tiles { get; init; }

    public required IReadOnlyList<MetricTable> Tables { get; init; }

    public required IReadOnlyList<MetricFamilyInfo> Families { get; init; }
}
