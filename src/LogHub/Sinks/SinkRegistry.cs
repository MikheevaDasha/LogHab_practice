using LogHub.Config;

namespace LogHub.Sinks;

public sealed class SinkRegistry
{
    private readonly Dictionary<string, ILogSinkFactory> _byName;

    public SinkRegistry(IEnumerable<ILogSinkFactory> factories)
    {
        _byName = factories.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
    }

    public ILogSink Create(LogHubConfig config)
    {
        if (_byName.TryGetValue(config.Sink, out var factory))
        {
            return factory.Create(config);
        }

        throw new ArgumentException(
            $"Неизвестный приёмник '{config.Sink}'. Доступны: {string.Join(", ", _byName.Keys)}.");
    }
}
