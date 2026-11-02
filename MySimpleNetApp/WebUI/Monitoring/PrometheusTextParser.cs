using System.Globalization;
using System.Text;

namespace MySimpleNetApp.WebUI.Monitoring;

/// <summary>
/// Minimalny parser formatu tekstowego Prometheusa (# HELP / # TYPE / probki).
/// Wystarcza do zbudowania dashboardu w aplikacji bez ciagniecia klienta Prometheusa.
/// </summary>
public static class PrometheusTextParser
{
    private static readonly IReadOnlyDictionary<string, string> NoLabels =
        new Dictionary<string, string>();

    public static MetricsSnapshot Parse(string text)
    {
        var samples = new List<MetricSample>();
        var help = new Dictionary<string, string>(StringComparer.Ordinal);
        var types = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r').Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line[0] == '#')
            {
                ParseMetadata(line, help, types);
                continue;
            }

            var sample = ParseSample(line);
            if (sample is not null)
            {
                samples.Add(sample);
            }
        }

        var families = BuildFamilies(help, types, samples);
        return new MetricsSnapshot(samples, families);
    }

    private static void ParseMetadata(
        string line,
        Dictionary<string, string> help,
        Dictionary<string, string> types)
    {
        var parts = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return;
        }

        var kind = parts[1];
        var name = parts[2];

        if (kind == "HELP")
        {
            help[name] = parts.Length > 3 ? UnescapeHelp(parts[3]) : string.Empty;
        }
        else if (kind == "TYPE")
        {
            types[name] = parts.Length > 3 ? parts[3] : "untyped";
        }
    }

    private static MetricSample? ParseSample(string line)
    {
        var braceStart = line.IndexOf('{');
        string name;
        IReadOnlyDictionary<string, string> labels;
        string rest;

        if (braceStart >= 0)
        {
            var braceEnd = line.LastIndexOf('}');
            if (braceEnd <= braceStart)
            {
                return null;
            }

            name = line[..braceStart];
            labels = ParseLabels(line[(braceStart + 1)..braceEnd]);
            rest = line[(braceEnd + 1)..].Trim();
        }
        else
        {
            var space = line.IndexOf(' ');
            if (space < 0)
            {
                return null;
            }

            name = line[..space];
            labels = NoLabels;
            rest = line[(space + 1)..].Trim();
        }

        var valueText = rest.Split(' ')[0];
        if (!TryParseValue(valueText, out var value))
        {
            return null;
        }

        return new MetricSample(name, labels, value);
    }

    private static Dictionary<string, string> ParseLabels(string text)
    {
        var labels = new Dictionary<string, string>(StringComparer.Ordinal);
        var current = new StringBuilder();
        var parts = new List<string>();
        var inQuotes = false;

        foreach (var ch in text)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                current.Append(ch);
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                parts.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString());
        }

        foreach (var part in parts)
        {
            var separator = part.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = part[..separator].Trim();
            var value = part[(separator + 1)..].Trim().Trim('"');
            labels[key] = UnescapeLabelValue(value);
        }

        return labels;
    }

    private static List<MetricFamilyInfo> BuildFamilies(
        Dictionary<string, string> help,
        Dictionary<string, string> types,
        List<MetricSample> samples)
    {
        var names = new SortedSet<string>(types.Keys, StringComparer.Ordinal);
        names.UnionWith(help.Keys);

        var families = new List<MetricFamilyInfo>();
        foreach (var family in names)
        {
            // Histogramy i summary wystawiaja probski z sufiksami _bucket/_sum/_count.
            var count = samples.Count(s =>
                s.Name == family ||
                s.Name.StartsWith(family + "_", StringComparison.Ordinal));

            types.TryGetValue(family, out var type);
            help.TryGetValue(family, out var familyHelp);

            families.Add(new MetricFamilyInfo(
                family,
                type ?? "untyped",
                familyHelp ?? string.Empty,
                count));
        }

        return families;
    }

    private static bool TryParseValue(string text, out double value)
    {
        var normalized = text switch
        {
            "+Inf" => "Infinity",
            "-Inf" => "-Infinity",
            _ => text
        };

        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static string UnescapeHelp(string text) =>
        text.Replace("\\n", " ").Replace("\\\\", "\\").Trim();

    private static string UnescapeLabelValue(string text) =>
        text.Replace("\\\"", "\"").Replace("\\\\", "\\");
}
