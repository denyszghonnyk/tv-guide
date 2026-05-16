namespace TVProgram.Domain;

/// <summary>
/// Базовий запис для представлення результату операції.
/// </summary>
internal abstract record Result<T>
{
    /// <summary>
    /// Успішний результат, що містить дані.
    /// </summary>
    internal sealed record Success(T Value) : Result<T>;

    /// <summary>
    /// Результат помилки, що містить повідомлення.
    /// </summary>
    internal sealed record Failure(string Message) : Result<T>;
}