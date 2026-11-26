using System.Text.Json;
using LogHub.Domain;
using LogHub.Formatting;

namespace LogHub.Tests;

public class FormatterTests
{
    private static readonly LogRecord Sample =
        new(new DateTime(2026, 11, 26, 10, 15, 0), Severity.Warning, "Диск почти полон");

    [Fact]
    public void PlainFormatter_ProducesSourceFormat()
    {
        Assert.Equal("[2026-11-26 10:15:00] WARNING Диск почти полон", new PlainFormatter().Format(Sample));
    }

    [Fact]
    public void MessageOnlyFormatter_ReturnsMessage()
    {
        Assert.Equal("Диск почти полон", new MessageOnlyFormatter().Format(Sample));
    }

    [Fact]
    public void JsonFormatter_ProducesValidJsonWithAllFields()
    {
        using var doc = JsonDocument.Parse(new JsonFormatter().Format(Sample));

        Assert.Equal("2026-11-26T10:15:00", doc.RootElement.GetProperty("time").GetString());
        Assert.Equal("Warning", doc.RootElement.GetProperty("level").GetString());
        Assert.Equal("Диск почти полон", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public void Registry_FindsFormatterByNameIgnoringCase()
    {
        var registry = new FormatterRegistry(new ILogFormatter[] { new PlainFormatter(), new JsonFormatter() });

        Assert.IsType<JsonFormatter>(registry.Get("JSON"));
    }

    [Fact]
    public void Registry_UnknownName_ThrowsWithAvailableNames()
    {
        var registry = new FormatterRegistry(new ILogFormatter[] { new PlainFormatter() });

        var ex = Assert.Throws<ArgumentException>(() => registry.Get("xml"));
        Assert.Contains("plain", ex.Message);
    }
}
