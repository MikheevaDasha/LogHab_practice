using LogHub.Config;
using LogHub.Processing;
using LogHub.Sinks;
using LogHub.Tests.Fakes;

namespace LogHub.Tests;

public class SinkRegistryTests
{
    [Fact]
    public void Create_PicksFactoryByConfigName()
    {
        var console = new RecordingSink();
        var file = new RecordingSink();
        var registry = new SinkRegistry(new ILogSinkFactory[]
        {
            new FakeSinkFactory("console", console),
            new FakeSinkFactory("file", file)
        });

        var sink = registry.Create(LogHubConfig.Parse(new[] { "sink=FILE" }));

        Assert.Same(file, sink);
    }

    [Fact]
    public void Create_UnknownSink_ThrowsWithAvailableNames()
    {
        var registry = new SinkRegistry(new ILogSinkFactory[] { new FakeSinkFactory("console", new RecordingSink()) });

        var ex = Assert.Throws<ArgumentException>(() => registry.Create(LogHubConfig.Parse(new[] { "sink=syslog" })));
        Assert.Contains("console", ex.Message);
    }

    [Fact]
    public void NewSink_IsAddedWithoutChangingExistingCode()
    {
        // Фабрика «memory» живёт только в тестовом проекте: ни один файл LogHub не менялся.
        var extra = new RecordingSink();
        var factories = PluginCatalog.Discover<ILogSinkFactory>()
            .Append<ILogSinkFactory>(new FakeSinkFactory("memory", extra));
        var registry = new SinkRegistry(factories);

        var sink = registry.Create(LogHubConfig.Parse(new[] { "sink=memory" }));

        Assert.Same(extra, sink);
    }

    [Fact]
    public void PluginCatalog_FindsBuiltInSinkFactories()
    {
        var names = PluginCatalog.Discover<ILogSinkFactory>().Select(f => f.Name).ToList();

        Assert.Contains("console", names);
        Assert.Contains("file", names);
    }

    [Fact]
    public void PluginCatalog_FindsBuiltInFormatters()
    {
        var names = PluginCatalog.Discover<LogHub.Formatting.ILogFormatter>().Select(f => f.Name).ToList();

        Assert.Contains("plain", names);
        Assert.Contains("json", names);
        Assert.Contains("message", names);
    }
}
