namespace MySimpleNetApp.WebUI.Monitoring;

public sealed record MetricSample(
    string Name,
    IReadOnlyDictionary<string, string> Labels,
    double Value);

public sealed record MetricFamilyInfo(
    string Name,
    string Type,
    string Help,
    int SampleCount);

public sealed record MetricsSnapshot(
    IReadOnlyList<MetricSample> Samples,
    IReadOnlyList<MetricFamilyInfo> Families);
