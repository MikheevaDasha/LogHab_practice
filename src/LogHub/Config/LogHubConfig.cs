using System.Globalization;
using LogHub.Domain;

namespace LogHub.Config;

/// <summary>Настройки в формате «ключ=значение». Нет ключа — берётся значение по умолчанию.</summary>
public sealed class LogHubConfig
{
    public string Sink { get; private init; } = "console";
    public string Format { get; private init; } = "plain";
    public Severity MinSeverity { get; private init; } = Severity.Debug;
    public string FilePath { get; private init; } = "loghub.out.log";
    public long MaxFileBytes { get; private init; } = 1_048_576;
    public int MaxBackups { get; private init; } = 3;

    public static LogHubConfig Parse(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq <= 0)
            {
                continue;
            }

            values[line[..eq].Trim()] = line[(eq + 1)..].Trim();
        }

        var defaults = new LogHubConfig();
        return new LogHubConfig
        {
            Sink = values.GetValueOrDefault("sink", defaults.Sink),
            Format = values.GetValueOrDefault("format", defaults.Format),
            MinSeverity = ParseSeverity(values.GetValueOrDefault("minSeverity"), defaults.MinSeverity),
            FilePath = values.GetValueOrDefault("filePath", defaults.FilePath),
            MaxFileBytes = ParseNumber(values.GetValueOrDefault("maxFileBytes"), "maxFileBytes", defaults.MaxFileBytes),
            MaxBackups = (int)ParseNumber(values.GetValueOrDefault("maxBackups"), "maxBackups", defaults.MaxBackups)
        };
    }

    private static Severity ParseSeverity(string? text, Severity fallback)
    {
        if (text is null)
        {
            return fallback;
        }

        if (Enum.TryParse<Severity>(text, ignoreCase: true, out var value) && Enum.IsDefined(value))
        {
            return value;
        }

        throw new FormatException($"Неизвестный уровень в minSeverity: '{text}'.");
    }

    private static long ParseNumber(string? text, string key, long fallback)
    {
        if (text is null)
        {
            return fallback;
        }

        if (long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new FormatException($"Ключ {key} должен быть целым неотрицательным числом, получено '{text}'.");
    }
}
