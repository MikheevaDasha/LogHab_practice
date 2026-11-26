using System.Text;

namespace LogHub.Sinks;

/// <summary>Всё, что приёмнику нужно от файловой системы. В тестах заменяется записью в памяти.</summary>
public interface ILogFileStore
{
    bool Exists(string path);

    /// <summary>Размер в байтах; 0, если файла нет.</summary>
    long Length(string path);

    void Append(string path, string text);

    void Delete(string path);

    void Move(string from, string to);
}

public sealed class DiskLogFileStore : ILogFileStore
{
    public bool Exists(string path) => File.Exists(path);

    public long Length(string path) => File.Exists(path) ? new FileInfo(path).Length : 0;

    public void Append(string path, string text) => File.AppendAllText(path, text, new UTF8Encoding(false));

    public void Delete(string path) => File.Delete(path);

    public void Move(string from, string to) => File.Move(from, to, overwrite: true);
}
