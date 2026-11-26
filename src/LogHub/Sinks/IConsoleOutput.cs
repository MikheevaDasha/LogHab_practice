namespace LogHub.Sinks;

public interface IConsoleOutput
{
    void WriteLine(string text);
}

public sealed class SystemConsoleOutput : IConsoleOutput
{
    public void WriteLine(string text) => Console.WriteLine(text);
}
