using LogHub.Domain;
using LogHub.Parsing;

namespace LogHub.Tests;

public class PlainTextLogParserTests
{
    private readonly PlainTextLogParser _parser = new();

    [Theory]
    [InlineData("DEBUG", Severity.Debug)]
    [InlineData("INFO", Severity.Info)]
    [InlineData("WARNING", Severity.Warning)]
    [InlineData("WARN", Severity.Warning)]
    [InlineData("ERROR", Severity.Error)]
    [InlineData("CRITICAL", Severity.Critical)]
    [InlineData("error", Severity.Error)]
    public void TryParse_KnownLevel_ReturnsSeverity(string level, Severity expected)
    {
        var ok = _parser.TryParse($"[2026-11-26 10:15:00] {level} что-то случилось", out var record);

        Assert.True(ok);
        Assert.Equal(expected, record.Severity);
    }

    [Fact]
    public void TryParse_ValidLine_FillsAllFields()
    {
        var ok = _parser.TryParse("[2026-11-26 10:15:30] ERROR Disk is full on /dev/sda1", out var record);

        Assert.True(ok);
        Assert.Equal(new DateTime(2026, 11, 26, 10, 15, 30), record.Timestamp);
        Assert.Equal(Severity.Error, record.Severity);
        Assert.Equal("Disk is full on /dev/sda1", record.Message);
    }

    [Fact]
    public void TryParse_MessageWithBrackets_KeepsMessageIntact()
    {
        var ok = _parser.TryParse("[2026-11-26 10:15:30] INFO user [admin] logged in", out var record);

        Assert.True(ok);
        Assert.Equal("user [admin] logged in", record.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("просто текст без структуры")]
    [InlineData("2026-11-26 10:15:00 ERROR нет скобок вокруг даты")]
    [InlineData("[2026-13-40 99:99:99] ERROR невозможная дата")]
    [InlineData("[26.11.2026 10:15:00] ERROR другой формат даты")]
    [InlineData("[2026-11-26 10:15:00] FATALITY неизвестный уровень")]
    [InlineData("[2026-11-26 10:15:00] ERROR")]
    [InlineData("[2026-11-26 10:15:00]")]
    public void TryParse_BadLine_ReturnsFalse(string line)
    {
        var ok = _parser.TryParse(line, out _);

        Assert.False(ok);
    }
}
