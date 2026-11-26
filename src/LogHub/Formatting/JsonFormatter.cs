using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using LogHub.Domain;

namespace LogHub.Formatting;

public sealed class JsonFormatter : ILogFormatter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string Name => "json";

    public string Format(LogRecord record) => JsonSerializer.Serialize(new
    {
        time = record.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        level = record.Severity.ToString(),
        message = record.Message
    }, Options);
}
