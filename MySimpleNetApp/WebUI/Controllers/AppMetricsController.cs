using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using MySimpleNetApp.WebUI.Data;
using MySimpleNetApp.WebUI.Models;
using MySimpleNetApp.WebUI.Monitoring;

namespace MySimpleNetApp.WebUI.Controllers;

public class AppMetricsController : Controller
{
    private readonly IMetricsSnapshotProvider _metrics;
    private readonly IDatabaseConnectionChecker _database;
    private readonly ITodoRepository _todos;

    public AppMetricsController(
        IMetricsSnapshotProvider metrics,
        IDatabaseConnectionChecker database,
        ITodoRepository todos)
    {
        _metrics = metrics;
        _database = database;
        _todos = todos;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var snapshot = await _metrics.GetAsync(cancellationToken);
        var connected = await _database.IsConnectedAsync(cancellationToken);
        var taskCount = (await _todos.GetAllAsync(cancellationToken)).Count;

        return View(BuildModel(snapshot, connected, taskCount));
    }

    private static AppMetricsViewModel BuildModel(MetricsSnapshot snapshot, bool connected, int taskCount)
    {
        var startSeconds = FirstValue(snapshot, "process_start_time_seconds");
        var uptime = startSeconds is null
            ? "n/a"
            : FormatDuration(DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds((long)startSeconds.Value));

        var totalRequests = Sum(snapshot, "http_requests_received_total");
        var failedRequests = snapshot.Samples
            .Where(s => s.Name == "http_requests_received_total"
                        && s.Labels.TryGetValue("code", out var code)
                        && code.StartsWith('5'))
            .Sum(s => s.Value);

        var tiles = new List<MetricTile>
        {
            new("Database", connected ? "Connected" : "Disconnected",
                connected ? null : "check the db container"),
            new("Tasks in database", FormatCount(taskCount)),
            new("Uptime", uptime),
            new("Working set", FormatBytes(FirstValue(snapshot, "process_working_set_bytes"))),
            new("HTTP requests", FormatCount(totalRequests)),
            new("HTTP 5xx", FormatCount(failedRequests)),
            new("GC collections", FormatCount(Sum(snapshot, "dotnet_collection_count_total"))),
            new("Threads", FormatCount(FirstValue(snapshot, "process_num_threads")))
        };

        var tables = new List<MetricTable>
        {
            BuildRoutesTable(snapshot, totalRequests),
            BuildStatusTable(snapshot, totalRequests),
            BuildProcessTable(snapshot),
            BuildDatabaseTable(snapshot)
        };

        return new AppMetricsViewModel
        {
            GeneratedAt = DateTimeOffset.Now,
            Tiles = tiles,
            Tables = tables,
            Families = snapshot.Families
        };
    }

    private static MetricTable BuildRoutesTable(MetricsSnapshot snapshot, double totalRequests)
    {
        var rows = snapshot.Samples
            .Where(s => s.Name == "http_requests_received_total")
            .GroupBy(s => (
                Route: Label(s, "controller") + "/" + Label(s, "action"),
                Method: Label(s, "method"),
                Code: Label(s, "code")))
            .Select(g => new { g.Key, Count = g.Sum(x => x.Value) })
            .OrderByDescending(x => x.Count)
            .Select(x => (IReadOnlyList<string>)new List<string>
            {
                x.Key.Route,
                x.Key.Method,
                x.Key.Code,
                FormatCount(x.Count),
                totalRequests > 0 ? (x.Count / totalRequests).ToString("P0", CultureInfo.InvariantCulture) : "-"
            })
            .ToList();

        return new MetricTable(
            "HTTP requests by route",
            ["Route", "Method", "Status", "Requests", "Share"],
            rows);
    }

    private static MetricTable BuildStatusTable(MetricsSnapshot snapshot, double totalRequests)
    {
        var rows = snapshot.Samples
            .Where(s => s.Name == "http_requests_received_total")
            .GroupBy(s => Label(s, "code"))
            .Select(g => new { Code = g.Key, Count = g.Sum(x => x.Value) })
            .OrderBy(x => x.Code)
            .Select(x => (IReadOnlyList<string>)new List<string>
            {
                x.Code,
                FormatCount(x.Count),
                totalRequests > 0 ? (x.Count / totalRequests).ToString("P0", CultureInfo.InvariantCulture) : "-"
            })
            .ToList();

        return new MetricTable("HTTP requests by status", ["Status", "Requests", "Share"], rows);
    }

    private static MetricTable BuildProcessTable(MetricsSnapshot snapshot)
    {
        var rows = new List<IReadOnlyList<string>>();

        Add("CPU time", FormatSeconds(FirstValue(snapshot, "process_cpu_seconds_total")));
        Add("Working set", FormatBytes(FirstValue(snapshot, "process_working_set_bytes")));
        Add("Private memory", FormatBytes(FirstValue(snapshot, "process_private_memory_bytes")));
        Add("Virtual memory", FormatBytes(FirstValue(snapshot, "process_virtual_memory_bytes")));
        Add("Threads", FormatCount(FirstValue(snapshot, "process_num_threads")));
        Add("Open handles", FormatCount(FirstValue(snapshot, "process_open_handles")));

        var start = FirstValue(snapshot, "process_start_time_seconds");
        if (start is not null)
        {
            var startedAt = DateTimeOffset.FromUnixTimeSeconds((long)start.Value).ToLocalTime();
            Add("Started at", startedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        return new MetricTable("Process", ["Metric", "Value"], rows);

        void Add(string label, string? value)
        {
            if (value is not null)
            {
                rows.Add(new List<string> { label, value });
            }
        }
    }

    private static MetricTable BuildDatabaseTable(MetricsSnapshot snapshot)
    {
        var rows = new List<IReadOnlyList<string>>();

        Add("Connections in use", FormatCount(FirstValue(snapshot, "npgsql_db_client_connections_usage")));
        Add("Max pool size", FormatCount(FirstValue(snapshot, "npgsql_db_client_connections_max")));
        Add("Commands executing", FormatCount(FirstValue(snapshot, "npgsql_db_client_commands_executing")));
        Add("Bytes read", FormatBytes(FirstValue(snapshot, "npgsql_db_client_commands_bytes_read")));
        Add("Bytes written", FormatBytes(FirstValue(snapshot, "npgsql_db_client_commands_bytes_written")));

        return new MetricTable("Database client (Npgsql)", ["Metric", "Value"], rows);

        void Add(string label, string? value)
        {
            if (value is not null)
            {
                rows.Add(new List<string> { label, value });
            }
        }
    }

    private static double Sum(MetricsSnapshot snapshot, string name) =>
        snapshot.Samples.Where(s => s.Name == name).Sum(s => s.Value);

    private static double? FirstValue(MetricsSnapshot snapshot, string name) =>
        snapshot.Samples.FirstOrDefault(s => s.Name == name)?.Value;

    private static string Label(MetricSample sample, string key) =>
        sample.Labels.TryGetValue(key, out var value) ? value : "-";

    private static string FormatCount(double? value) =>
        value is null ? "n/a" : ((long)value.Value).ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatCount(int value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatBytes(double? value)
    {
        if (value is null)
        {
            return "n/a";
        }

        const double megabyte = 1024 * 1024;
        return (value.Value / megabyte).ToString("N1", CultureInfo.InvariantCulture) + " MB";
    }

    private static string FormatSeconds(double? value) =>
        value is null ? "n/a" : value.Value.ToString("N2", CultureInfo.InvariantCulture) + " s";

    private static string FormatDuration(TimeSpan span)
    {
        if (span < TimeSpan.Zero)
        {
            span = TimeSpan.Zero;
        }

        var parts = new List<string>();
        if (span.Days > 0)
        {
            parts.Add($"{span.Days}d");
        }

        if (span.Hours > 0)
        {
            parts.Add($"{span.Hours}h");
        }

        parts.Add($"{span.Minutes}m");
        parts.Add($"{span.Seconds}s");
        return string.Join(' ', parts);
    }
}
