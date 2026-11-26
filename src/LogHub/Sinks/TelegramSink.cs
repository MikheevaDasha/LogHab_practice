using LogHub.Config;
using LogHub.Domain;

namespace LogHub.Sinks;

/// <summary>
/// Демонстрация OCP: приёмник добавлен одним новым файлом, существующий код не менялся.
/// Токен и чат берутся из переменных окружения, поэтому LogHubConfig править не пришлось.
/// </summary>
public sealed class TelegramSink : ILogSink
{
    private readonly HttpClient _client = new();
    private readonly string _url;
    private readonly string _chatId;

    public TelegramSink(string botToken, string chatId)
    {
        _url = $"https://api.telegram.org/bot{botToken}/sendMessage";
        _chatId = chatId;
    }

    public void Write(LogRecord record, string text)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["chat_id"] = _chatId,
            ["text"] = text
        });
        _client.PostAsync(_url, content).GetAwaiter().GetResult().EnsureSuccessStatusCode();
    }

    public void Dispose() => _client.Dispose();
}

public sealed class TelegramSinkFactory : ILogSinkFactory
{
    public string Name => "telegram";

    public ILogSink Create(LogHubConfig config)
    {
        var token = Environment.GetEnvironmentVariable("LOGHUB_TELEGRAM_TOKEN");
        var chat = Environment.GetEnvironmentVariable("LOGHUB_TELEGRAM_CHAT");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chat))
        {
            throw new ArgumentException("Для приёмника telegram задайте LOGHUB_TELEGRAM_TOKEN и LOGHUB_TELEGRAM_CHAT.");
        }

        return new TelegramSink(token, chat);
    }

}
