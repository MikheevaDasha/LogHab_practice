namespace LogHub.Domain;

public sealed record LogRecord(DateTime Timestamp, Severity Severity, string Message);
