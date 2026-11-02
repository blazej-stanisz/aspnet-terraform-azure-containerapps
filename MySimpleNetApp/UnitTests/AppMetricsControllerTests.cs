using Microsoft.AspNetCore.Mvc;
using MySimpleNetApp.WebUI.Controllers;
using MySimpleNetApp.WebUI.Models;
using MySimpleNetApp.WebUI.Monitoring;

namespace MySimpleNetApp.UnitTests;

public class AppMetricsControllerTests
{
    private const string MetricsText = """
        # HELP http_requests_received_total Provides the count of HTTP requests.
        # TYPE http_requests_received_total counter
        http_requests_received_total{code="200",controller="Home",action="Index",method="GET"} 4
        http_requests_received_total{code="200",controller="Home",action="TodoList",method="GET"} 2
        # HELP process_num_threads Number of threads.
        # TYPE process_num_threads gauge
        process_num_threads 12
        # HELP process_working_set_bytes Process working set
        # TYPE process_working_set_bytes gauge
        process_working_set_bytes 104857600
        """;

    private static async Task<AppMetricsController> CreateControllerAsync(bool databaseConnected, int tasks)
    {
        var repository = new FakeTodoRepository();
        for (var i = 0; i < tasks; i++)
        {
            await repository.AddAsync($"zadanie {i + 1}");
        }

        var snapshot = PrometheusTextParser.Parse(MetricsText);
        return new AppMetricsController(
            new FakeMetricsSnapshotProvider(snapshot),
            new FakeDatabaseConnectionChecker(databaseConnected),
            repository);
    }

    [Fact]
    public async Task Index_WhenCalled_ReturnsViewWithStatusTiles()
    {
        var controller = await CreateControllerAsync(databaseConnected: true, tasks: 2);

        var result = await controller.Index(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AppMetricsViewModel>(view.Model);

        Assert.Contains(model.Tiles, t => t.Label == "Database" && t.Value == "Connected");
        Assert.Contains(model.Tiles, t => t.Label == "Tasks in database" && t.Value == "2");
        Assert.Contains(model.Tiles, t => t.Label == "HTTP requests" && t.Value == "6");
        Assert.Contains(model.Tiles, t => t.Label == "Working set" && t.Value == "100.0 MB");
    }

    [Fact]
    public async Task Index_WhenDatabaseIsDown_ShowsDisconnectedTile()
    {
        var controller = await CreateControllerAsync(databaseConnected: false, tasks: 0);

        var result = await controller.Index(CancellationToken.None);

        var model = Assert.IsType<AppMetricsViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Contains(model.Tiles, t => t.Label == "Database" && t.Value == "Disconnected");
    }

    [Fact]
    public async Task Index_WhenRequestsExist_GroupsThemByRoute()
    {
        var controller = await CreateControllerAsync(databaseConnected: true, tasks: 0);

        var result = await controller.Index(CancellationToken.None);

        var model = Assert.IsType<AppMetricsViewModel>(Assert.IsType<ViewResult>(result).Model);
        var routes = model.Tables.Single(t => t.Title == "HTTP requests by route");

        Assert.Equal(2, routes.Rows.Count);
        Assert.Equal("Home/Index", routes.Rows[0][0]);
        Assert.Equal("4", routes.Rows[0][3]);
    }
}
