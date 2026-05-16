using System;
using System.Text.Json.Serialization;

namespace TVProgram.Domain;

/// <summary>
/// Представляє окрему телевізійну передачу з її атрибутами та часовими характеристиками.
/// Реалізує інтерфейс <see cref="IComparable{T}"/> для забезпечення автоматичного хронологічного сортування.
/// </summary>
internal record TVShow : IComparable<TVShow>
{
    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="TVShow"/> із заданими параметрами.
    /// Помічений атрибутом <see cref="JsonConstructorAttribute"/> для коректного відновлення об'єкта з JSON.
    /// </summary>
    /// <param name="Id">Унікальний ідентифікатор передачі.</param>
    /// <param name="Title">Назва телепередачі.</param>
    /// <param name="StartTime">Дата та час початку трансляції.</param>
    /// <param name="Duration">Загальна тривалість ефірного часу передачі.</param>
    /// <param name="Genre">Жанрова приналежність передачі.</param>
    [JsonConstructor]
    public TVShow(Guid Id, string Title, DateTime StartTime, TimeSpan Duration, Genre Genre)
    {
        this.Id = Id;
        this.Title = Title;
        this.StartTime = StartTime;
        this.Duration = Duration;
        this.Genre = Genre;
    }

    /// <summary>
    /// Унікальний глобальний ідентифікатор телепередачі (GUID).
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Назва телепередачі.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Дата та час початку ефіру телепередачі.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Тривалість трансляції телепередачі.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Жанр телепередачі (наприклад: Новини, Спорт, Фільм тощо).
    /// </summary>
    public Genre Genre { get; init; }

    /// <summary>
    /// Обчислювана властивість, яка повертає точний час закінчення передачі.
    /// Помічена <see cref="JsonIgnoreAttribute"/>, оскільки розраховується динамічно і не потребує збереження у файл.
    /// </summary>
    [JsonIgnore]
    public DateTime EndTime => StartTime + Duration;

    /// <summary>
    /// Реалізує порівняння поточного екземпляра з іншою телепередачею для сортування за часом початку.
    /// </summary>
    /// <param name="other">Об'єкт телепередачі для порівняння з поточним екземпляром.</param>
    /// <returns>
    /// Число менше нуля, якщо поточна передача починається раніше; 
    /// нуль, якщо часи початку збігаються; 
    /// число більше нуля, якщо вона починається пізніше або якщо об'єкт для порівняння є null.
    /// </returns>
    public int CompareTo(TVShow? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return StartTime.CompareTo(other.StartTime);
    }

    public static bool operator <(TVShow? left, TVShow? right) => left is null ? right is not null : left.CompareTo(right) < 0;
    public static bool operator <=(TVShow? left, TVShow? right) => left is null || left.CompareTo(right) <= 0;
    public static bool operator >(TVShow? left, TVShow? right) => left is not null && left.CompareTo(right) > 0;
    public static bool operator >=(TVShow? left, TVShow? right) => left is null ? right is null : left.CompareTo(right) >= 0;
}