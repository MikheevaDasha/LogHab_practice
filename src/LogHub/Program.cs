using LogHub.Config;
using LogHub.Domain;
using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;

// Композиционный корень: единственное место, где собираются объекты и читается диск.
if (args.Length == 0)
{
    Console.Error.WriteLine("Использование: LogHub <файл-лога> [файл-конфигурации]");
    return 1;
}

var logPath = args[0];
var configPath = args.Length > 1 ? args[1] : "loghub.config";

try
{
    var configLines = File.Exists(configPath) ? File.ReadAllLines(configPath) : Array.Empty<string>();
    var config = LogHubConfig.Parse(configLines);

    var sinks = new SinkRegistry(PluginCatalog.Discover<ILogSinkFactory>());
    var formatters = new FormatterRegistry(PluginCatalog.Discover<ILogFormatter>());

    ProcessingResult result;
    using (var sink = sinks.Create(config))
    {
        var processor = new LogProcessor(
            new PlainTextLogParser(),
            formatters.Get(config.Format),
            sink,
            config.MinSeverity);
        result = processor.Process(File.ReadLines(logPath));
    }

    Console.WriteLine($"Записано: {result.Written}, пропущено: {result.Skipped}");
    return 0;
}
catch (Exception ex) when (ex is ArgumentException or FormatException or IOException)
{
    Console.Error.WriteLine($"Ошибка: {ex.Message}");
    return 2;
}
