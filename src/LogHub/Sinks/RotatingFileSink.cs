using System.Text;
using LogHub.Config;
using LogHub.Domain;

namespace LogHub.Sinks;

/// <summary>
/// Пишет в файл и при превышении размера переименовывает его: app.log → app.log.1 → app.log.2 …
/// Старше maxBackups копий не хранится.
/// </summary>
public sealed class RotatingFileSink : ILogSink
{
    private const string LineEnd = "\n";

    private readonly ILogFileStore _store;
    private readonly string _path;
    private readonly long _maxBytes;
    private readonly int _maxBackups;

    public RotatingFileSink(ILogFileStore store, string path, long maxBytes, int maxBackups)
    {
        if (maxBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBytes), "Максимальный размер файла должен быть больше нуля.");
        }

        if (maxBackups < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBackups), "Нужна хотя бы одна резервная копия.");
        }

        _store = store;
        _path = path;
        _maxBytes = maxBytes;
        _maxBackups = maxBackups;
    }

    public void Write(LogRecord record, string text)
    {
        var line = text + LineEnd;
        var size = _store.Length(_path);

        // size > 0: одна запись больше лимита не должна вызывать ротацию пустого файла.
        if (size > 0 && size + Encoding.UTF8.GetByteCount(line) > _maxBytes)
        {
            Rotate();
        }

        _store.Append(_path, line);
    }

    private void Rotate()
    {
        var oldest = Backup(_maxBackups);
        if (_store.Exists(oldest))
        {
            _store.Delete(oldest);
        }

        for (var i = _maxBackups - 1; i >= 1; i--)
        {
            if (_store.Exists(Backup(i)))
            {
                _store.Move(Backup(i), Backup(i + 1));
            }
        }

        _store.Move(_path, Backup(1));
    }

    private string Backup(int index) => $"{_path}.{index}";

    public void Dispose()
    {
    }
}

public sealed class FileSinkFactory : ILogSinkFactory
{
    public string Name => "file";

    public ILogSink Create(LogHubConfig config) =>
        new RotatingFileSink(new DiskLogFileStore(), config.FilePath, config.MaxFileBytes, config.MaxBackups);
}
