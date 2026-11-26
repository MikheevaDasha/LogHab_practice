using LogHub.Config;
using LogHub.Domain;

namespace LogHub.Tests;

public class LogHubConfigTests
{
    [Fact]
    public void Parse_EmptyInput_GivesDefaults()
    {
        var config = LogHubConfig.Parse(Array.Empty<string>());

        Assert.Equal("console", config.Sink);
        Assert.Equal("plain", config.Format);
        Assert.Equal(Severity.Debug, config.MinSeverity);
        Assert.Equal(1_048_576, config.MaxFileBytes);
        Assert.Equal(3, config.MaxBackups);
    }

    [Fact]
    public void Parse_AllKeys_AreRead()
    {
        var config = LogHubConfig.Parse(new[]
        {
            "sink = file",
            "format=json",
            "minSeverity=warning",
            "filePath=out/app.log",
            "maxFileBytes=2048",
            "maxBackups=5"
        });

        Assert.Equal("file", config.Sink);
        Assert.Equal("json", config.Format);
        Assert.Equal(Severity.Warning, config.MinSeverity);
        Assert.Equal("out/app.log", config.FilePath);
        Assert.Equal(2048, config.MaxFileBytes);
        Assert.Equal(5, config.MaxBackups);
    }

    [Fact]
    public void Parse_CommentsBlankAndUnknownLines_AreIgnored()
    {
        var config = LogHubConfig.Parse(new[] { "# комментарий", "", "мусор без равно", "sink=file" });

        Assert.Equal("file", config.Sink);
    }

    [Fact]
    public void Parse_UnknownSeverity_Throws()
    {
        Assert.Throws<FormatException>(() => LogHubConfig.Parse(new[] { "minSeverity=loud" }));
    }

    [Theory]
    [InlineData("maxFileBytes=много")]
    [InlineData("maxFileBytes=-5")]
    [InlineData("maxBackups=1.5")]
    public void Parse_BadNumber_Throws(string line)
    {
        Assert.Throws<FormatException>(() => LogHubConfig.Parse(new[] { line }));
    }
}
