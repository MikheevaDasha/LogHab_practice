using LogHub.Domain;
using LogHub.Sinks;
using LogHub.Tests.Fakes;

namespace LogHub.Tests;

public class ConsoleSinkTests
{
    [Fact]
    public void Write_SendsTextToConsoleOutput()
    {
        var console = new RecordingConsole();
        var sink = new ConsoleSink(console);

        sink.Write(new LogRecord(DateTime.MinValue, Severity.Info, "m"), "готовая строка");

        Assert.Equal(new[] { "готовая строка" }, console.Lines);
    }
}
