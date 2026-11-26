using LogHub.Config;

namespace LogHub.Sinks;

public interface ILogSinkFactory
{
    string Name { get; }

    ILogSink Create(LogHubConfig config);
}
