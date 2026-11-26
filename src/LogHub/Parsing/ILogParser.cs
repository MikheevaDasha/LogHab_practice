using LogHub.Domain;

namespace LogHub.Parsing;

public interface ILogParser
{
    /// <summary>false — строка не соответствует формату.</summary>
    bool TryParse(string line, out LogRecord record);
}
