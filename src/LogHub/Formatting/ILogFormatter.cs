using LogHub.Domain;

namespace LogHub.Formatting;

public interface ILogFormatter
{
    /// <summary>Имя, по которому формат выбирается в конфигурации.</summary>
    string Name { get; }

    string Format(LogRecord record);
}
