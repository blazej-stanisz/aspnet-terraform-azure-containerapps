using MySimpleNetApp.WebUI.Monitoring;

namespace MySimpleNetApp.UnitTests;

public class PrometheusTextParserTests
{
    private const string Sample = """
        # HELP http_requests_received_total Provides the count of HTTP requests.
        # TYPE http_requests_received_total counter
        http_requests_received_total{code="200",controller="Home",action="Index",method="GET"} 3
        http_requests_received_total{code="500",controller="Home",action="Error",method="GET"} 1
        # HELP process_num_threads Number of threads.
        # TYPE process_num_threads gauge
        process_num_threads 12
        # TYPE http_request_duration_seconds histogram
        http_request_duration_seconds_bucket{le="0.005"} 2
        http_request_duration_seconds_sum 0.02
        """;

    [Fact]
    public void Parse_WhenTextHasLabels_ParsesNamesLabelsAndValues()
    {
        var snapshot = PrometheusTextParser.Parse(Sample);

        var first = snapshot.Samples.First();
        Assert.Equal("http_requests_received_total", first.Name);
        Assert.Equal("200", first.Labels["code"]);
        Assert.Equal("Index", first.Labels["action"]);
        Assert.Equal(3, first.Value);
        Assert.Equal(5, snapshot.Samples.Count);
    }

    [Fact]
    public void Parse_WhenHistogramHasSuffixes_CountsSamplesInFamily()
    {
        var snapshot = PrometheusTextParser.Parse(Sample);

        var family = snapshot.Families.Single(f => f.Name == "http_request_duration_seconds");
        Assert.Equal("histogram", family.Type);
        Assert.Equal(2, family.SampleCount);
    }

    [Fact]
    public void Parse_WhenMetadataPresent_KeepsHelpAndType()
    {
        var snapshot = PrometheusTextParser.Parse(Sample);

        var family = snapshot.Families.Single(f => f.Name == "process_num_threads");
        Assert.Equal("gauge", family.Type);
        Assert.Equal("Number of threads.", family.Help);
    }

    [Fact]
    public void Parse_WhenLineIsMalformed_SkipsIt()
    {
        var snapshot = PrometheusTextParser.Parse("to nie jest probka\n# TYPE x gauge\nx 1\n");

        Assert.Single(snapshot.Samples);
        Assert.Equal("x", snapshot.Samples[0].Name);
        Assert.Equal(1, snapshot.Samples[0].Value);
    }
}
