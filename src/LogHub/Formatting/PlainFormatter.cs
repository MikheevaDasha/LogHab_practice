using System.Globalization;
using LogHub.Domain;

namespace LogHub.Formatting;

public sealed class PlainFormatter : ILogFormatter
{
    public string Name => "plain";

    public string Format(LogRecord record) =>
        $"[{record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}] " +
        $"{record.Severity.ToString().ToUpperInvariant()} {record.Message}";
}
