using LogHub.Config;
using LogHub.Domain;

namespace LogHub.Sinks;

public sealed class ConsoleSink : ILogSink
{
    private readonly IConsoleOutput _output;

    public ConsoleSink(IConsoleOutput output) => _output = output;

    public void Write(LogRecord record, string text) => _output.WriteLine(text);

    public void Dispose()
    {
    }
}

public sealed class ConsoleSinkFactory : ILogSinkFactory
{
    public string Name => "console";

    public ILogSink Create(LogHubConfig config) => new ConsoleSink(new SystemConsoleOutput());
}
