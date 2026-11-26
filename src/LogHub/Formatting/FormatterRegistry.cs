namespace LogHub.Formatting;

public sealed class FormatterRegistry
{
    private readonly Dictionary<string, ILogFormatter> _byName;

    public FormatterRegistry(IEnumerable<ILogFormatter> formatters)
    {
        _byName = formatters.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
    }

    public ILogFormatter Get(string name)
    {
        if (_byName.TryGetValue(name, out var formatter))
        {
            return formatter;
        }

        throw new ArgumentException(
            $"Неизвестный формат вывода '{name}'. Доступны: {string.Join(", ", _byName.Keys)}.");
    }
}
