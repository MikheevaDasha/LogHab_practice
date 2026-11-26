using LogHub.Domain;
using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;
using LogHub.Tests.Fakes;

namespace LogHub.Tests;

public class LogProcessorTests
{
    private static readonly string[] OneOfEachLevel =
    {
        "[2026-11-26 10:00:00] DEBUG d",
        "[2026-11-26 10:00:01] INFO i",
        "[2026-11-26 10:00:02] WARNING w",
        "[2026-11-26 10:00:03] ERROR e",
        "[2026-11-26 10:00:04] CRITICAL c"
    };

    private static LogProcessor Create(ILogSink sink, Severity min = Severity.Debug, ILogFormatter? formatter = null) =>
        new(new PlainTextLogParser(), formatter ?? new PlainFormatter(), sink, min);

    [Fact]
    public void Process_ValidAndBadLines_CountsWrittenAndSkipped()
    {
        var sink = new RecordingSink();
        var lines = new[] { OneOfEachLevel[1], "мусор", "", OneOfEachLevel[3] };

        var result = Create(sink).Process(lines);

        Assert.Equal(new ProcessingResult(2, 2), result);
        Assert.Equal(2, sink.Written.Count);
    }

    [Theory]
    [InlineData(Severity.Debug, 5)]
    [InlineData(Severity.Info, 4)]
    [InlineData(Severity.Warning, 3)]
    [InlineData(Severity.Error, 2)]
    [InlineData(Severity.Critical, 1)]
    public void Process_MinSeverity_FiltersLowerLevels(Severity min, int expectedWritten)
    {
        var sink = new RecordingSink();

        var result = Create(sink, min).Process(OneOfEachLevel);

        Assert.Equal(expectedWritten, result.Written);
        Assert.Equal(5 - expectedWritten, result.Skipped);
    }

    [Fact]
    public void Process_EmptyInput_WritesNothing()
    {
        var sink = new RecordingSink();

        var result = Create(sink).Process(Array.Empty<string>());

        Assert.Equal(new ProcessingResult(0, 0), result);
        Assert.Empty(sink.Written);
    }

    [Fact]
    public void Process_PassesFormattedTextAndRecordToSink()
    {
        var sink = new RecordingSink();

        Create(sink, formatter: new MessageOnlyFormatter()).Process(new[] { OneOfEachLevel[2] });

        var (record, text) = Assert.Single(sink.Written);
        Assert.Equal(Severity.Warning, record.Severity);
        Assert.Equal("w", text);
    }

    [Fact]
    public void Process_RoutesToTheSinkItWasGiven()
    {
        var first = new RecordingSink();
        var second = new RecordingSink();

        Create(first).Process(new[] { OneOfEachLevel[0] });
        Create(second).Process(new[] { OneOfEachLevel[1], OneOfEachLevel[2] });

        Assert.Single(first.Written);
        Assert.Equal(2, second.Written.Count);
    }

    [Fact]
    public void Process_OnConsoleSink_PrintsFormattedLines()
    {
        var console = new RecordingConsole();

        Create(new ConsoleSink(console)).Process(new[] { OneOfEachLevel[3] });

        Assert.Equal(new[] { "[2026-11-26 10:00:03] ERROR e" }, console.Lines);
    }

    [Fact]
    public void Process_OnFileSink_RotatesWhenLimitExceeded()
    {
        var store = new InMemoryFileStore();
        // Строки plain-формата занимают 29–33 байта с переводом строки: в файл на 70 байт помещаются две.
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 70, maxBackups: 2);

        Create(sink).Process(OneOfEachLevel);

        Assert.True(store.Exists("app.log.1"));
        Assert.Contains("CRITICAL", store.Files["app.log"]);
    }
}
