using LogHub.Domain;
using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Sinks;

namespace LogHub.Processing;

public sealed class LogProcessor
{
    private readonly ILogParser _parser;
    private readonly ILogFormatter _formatter;
    private readonly ILogSink _sink;
    private readonly Severity _minSeverity;

    public LogProcessor(ILogParser parser, ILogFormatter formatter, ILogSink sink, Severity minSeverity)
    {
        _parser = parser;
        _formatter = formatter;
        _sink = sink;
        _minSeverity = minSeverity;
    }

    public ProcessingResult Process(IEnumerable<string> lines)
    {
        var written = 0;
        var skipped = 0;
        foreach (var line in lines)
        {
            if (!_parser.TryParse(line, out var record) || record.Severity < _minSeverity)
            {
                skipped++;
                continue;
            }

            _sink.Write(record, _formatter.Format(record));
            written++;
        }

        return new ProcessingResult(written, skipped);
    }
}
