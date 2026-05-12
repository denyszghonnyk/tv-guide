namespace TVProgram.Domain;

/// <summary>
/// Базовий запис для представлення результату операції.
/// </summary>
public abstract record Result<T>
{
    /// <summary>
    /// Успішний результат, що містить дані.
    /// </summary>
    public sealed record Success(T Value) : Result<T>;

    /// <summary>
    /// Результат помилки, що містить повідомлення.
    /// </summary>
    public sealed record Failure(string Message) : Result<T>;
}