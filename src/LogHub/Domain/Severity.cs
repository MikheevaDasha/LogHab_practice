namespace LogHub.Domain;

/// <summary>Уровни важности; порядок значений задаёт порядок сравнения.</summary>
public enum Severity
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}
