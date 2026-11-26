using LogHub.Domain;

namespace LogHub.Formatting;

public sealed class MessageOnlyFormatter : ILogFormatter
{
    public string Name => "message";

    public string Format(LogRecord record) => record.Message;
}
