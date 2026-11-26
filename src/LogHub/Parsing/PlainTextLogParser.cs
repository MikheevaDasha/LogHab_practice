using System.Globalization;
using System.Text.RegularExpressions;
using LogHub.Domain;

namespace LogHub.Parsing;

/// <summary>Формат варианта 1: «[2026-11-26 10:15:00] ERROR сообщение».</summary>
public sealed partial class PlainTextLogParser : ILogParser
{
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";

    [GeneratedRegex(@"^\[(?<ts>[^\]]+)\]\s+(?<level>[A-Za-z]+)\s+(?<msg>\S.*)$")]
    private static partial Regex LinePattern();

    public bool TryParse(string line, out LogRecord record)
    {
        record = null!;
        var match = LinePattern().Match(line.Trim());
        if (!match.Success)
        {
            return false;
        }

        if (!DateTime.TryParseExact(match.Groups["ts"].Value, TimestampFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
        {
            return false;
        }

        if (!TryParseSeverity(match.Groups["level"].Value, out var severity))
        {
            return false;
        }

        record = new LogRecord(timestamp, severity, match.Groups["msg"].Value);
        return true;
    }

    private static bool TryParseSeverity(string text, out Severity severity)
    {
        if (text.Equals("WARN", StringComparison.OrdinalIgnoreCase))
        {
            severity = Severity.Warning;
            return true;
        }

        return Enum.TryParse(text, ignoreCase: true, out severity) && Enum.IsDefined(severity);
    }
}
