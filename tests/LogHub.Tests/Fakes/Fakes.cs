using System.Text;
using LogHub.Config;
using LogHub.Domain;
using LogHub.Sinks;

namespace LogHub.Tests.Fakes;

/// <summary>Файловая система в памяти: тесты не трогают диск.</summary>
public sealed class InMemoryFileStore : ILogFileStore
{
    public Dictionary<string, string> Files { get; } = new();

    public bool Exists(string path) => Files.ContainsKey(path);

    public long Length(string path) => Files.TryGetValue(path, out var text) ? Encoding.UTF8.GetByteCount(text) : 0;

    public void Append(string path, string text) => Files[path] = Files.GetValueOrDefault(path, "") + text;

    public void Delete(string path) => Files.Remove(path);

    public void Move(string from, string to)
    {
        Files[to] = Files[from];
        Files.Remove(from);
    }
}

public sealed class RecordingConsole : IConsoleOutput
{
    public List<string> Lines { get; } = new();

    public void WriteLine(string text) => Lines.Add(text);
}

public sealed class RecordingSink : ILogSink
{
    public List<(LogRecord Record, string Text)> Written { get; } = new();

    public bool Disposed { get; private set; }

    public void Write(LogRecord record, string text) => Written.Add((record, text));

    public void Dispose() => Disposed = true;
}

public sealed class FakeSinkFactory : ILogSinkFactory
{
    private readonly ILogSink _sink;

    public FakeSinkFactory(string name, ILogSink sink)
    {
        Name = name;
        _sink = sink;
    }

    public string Name { get; }

    public ILogSink Create(LogHubConfig config) => _sink;
}
