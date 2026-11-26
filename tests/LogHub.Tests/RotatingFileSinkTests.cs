using LogHub.Domain;
using LogHub.Sinks;
using LogHub.Tests.Fakes;

namespace LogHub.Tests;

public class RotatingFileSinkTests
{
    private static readonly LogRecord Any = new(DateTime.MinValue, Severity.Info, "x");

    // Строка из 9 символов + "\n" = 10 байт.
    private const string Line9 = "123456789";

    [Fact]
    public void Write_BelowLimit_StaysInOneFile()
    {
        var store = new InMemoryFileStore();
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 100, maxBackups: 2);

        sink.Write(Any, Line9);
        sink.Write(Any, Line9);

        Assert.Equal(Line9 + "\n" + Line9 + "\n", store.Files["app.log"]);
        Assert.False(store.Exists("app.log.1"));
    }

    [Fact]
    public void Write_ExactlyAtLimit_DoesNotRotate()
    {
        var store = new InMemoryFileStore();
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 20, maxBackups: 2);

        sink.Write(Any, Line9);
        sink.Write(Any, Line9);

        Assert.False(store.Exists("app.log.1"));
    }

    [Fact]
    public void Write_OverLimit_MovesOldContentToBackup()
    {
        var store = new InMemoryFileStore();
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 20, maxBackups: 2);

        sink.Write(Any, "aaaaaaaaa");
        sink.Write(Any, "bbbbbbbbb");
        sink.Write(Any, "ccccccccc");

        Assert.Equal("aaaaaaaaa\nbbbbbbbbb\n", store.Files["app.log.1"]);
        Assert.Equal("ccccccccc\n", store.Files["app.log"]);
    }

    [Fact]
    public void Write_SeveralRotations_ShiftsBackupsAndDropsOldest()
    {
        var store = new InMemoryFileStore();
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 10, maxBackups: 2);

        sink.Write(Any, "aaaaaaaaa");
        sink.Write(Any, "bbbbbbbbb");
        sink.Write(Any, "ccccccccc");
        sink.Write(Any, "ddddddddd");

        Assert.Equal("ddddddddd\n", store.Files["app.log"]);
        Assert.Equal("ccccccccc\n", store.Files["app.log.1"]);
        Assert.Equal("bbbbbbbbb\n", store.Files["app.log.2"]);
        Assert.False(store.Exists("app.log.3"));
    }

    [Fact]
    public void Write_RecordLargerThanLimit_IsWrittenWithoutRotatingEmptyFile()
    {
        var store = new InMemoryFileStore();
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 5, maxBackups: 1);

        sink.Write(Any, "очень длинная запись");

        Assert.Equal("очень длинная запись\n", store.Files["app.log"]);
        Assert.False(store.Exists("app.log.1"));
    }

    [Fact]
    public void Write_CountsBytesNotCharacters()
    {
        var store = new InMemoryFileStore();
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 20, maxBackups: 1);

        // 6 кириллических букв = 12 байт + "\n" = 13 байт; вторая такая же уже не помещается.
        sink.Write(Any, "привет");
        sink.Write(Any, "привет");

        Assert.Equal("привет\n", store.Files["app.log.1"]);
    }

    [Fact]
    public void Write_ExistingFileFromPreviousRun_IsCountedTowardsLimit()
    {
        var store = new InMemoryFileStore();
        store.Files["app.log"] = "0123456789012345\n";
        var sink = new RotatingFileSink(store, "app.log", maxBytes: 20, maxBackups: 1);

        sink.Write(Any, Line9);

        Assert.Equal("0123456789012345\n", store.Files["app.log.1"]);
        Assert.Equal(Line9 + "\n", store.Files["app.log"]);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    public void Constructor_InvalidLimits_Throws(long maxBytes, int maxBackups)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RotatingFileSink(new InMemoryFileStore(), "app.log", maxBytes, maxBackups));
    }
}
