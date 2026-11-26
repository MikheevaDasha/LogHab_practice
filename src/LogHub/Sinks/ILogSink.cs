using LogHub.Domain;

namespace LogHub.Sinks;

public interface ILogSink : IDisposable
{
    void Write(LogRecord record, string text);
}
