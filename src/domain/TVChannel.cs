using TVProgram.Domain.Common;

namespace TVProgram.Domain.Entities;

/// <summary>
/// Представляє телеканал, що містить список запланованих передач.
/// </summary>
public class TVChannel
{
    private readonly List<TVShow> _shows = new();

    /// <summary>
    /// Унікальний ідентифікатор каналу.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Назва телеканалу.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Колекція передач каналу, доступна лише для читання.
    /// </summary>
    public IReadOnlyCollection<TVShow> Shows => _shows.AsReadOnly();

    /// <summary>
    /// Додає нову передачу до розкладу, перевіряючи її на відсутність часових конфліктів.
    /// </summary>
    /// <param name="newShow">Об'єкт передачі для додавання.</param>
    /// <returns>Результат операції: Success у разі успіху або Failure з описом колізії.</returns>
    public Result<TVShow> AddShow(TVShow newShow)
    {
        // Перевірка на колізії за формулою:
        // (StartA < EndB) && (StartB < EndA)
        var conflict = _shows.FirstOrDefault(existingShow => 
            newShow.StartTime < existingShow.EndTime && 
            existingShow.StartTime < newShow.EndTime);
    
        if (conflict is not null)
        {
            return new Result<TVShow>.Failure(
                $"Конфлікт розкладу: передача '{newShow.Title}' перетинається з " +
                $"'{conflict.Title}' ({conflict.StartTime:HH:mm} - {conflict.EndTime:HH:mm})."
            );
        }
    
        _shows.Add(newShow);
            
        // Автоматичне сортування завдяки реалізації IComparable в TVShow
        _shows.Sort();
    
        return new Result<TVShow>.Success(newShow);
    }
    
        /// <summary>
        /// Видаляє передачі з внутрішньої колекції за їхніми унікальними ідентифікаторами.
        /// </summary>
        /// <param name="ids">Перелік ідентифікаторів (Guid) передач, які потрібно видалити.</param>
        /// <returns>True, якщо хоча б одна передача була видалена; інакше — False.</returns>
    public bool RemoveShows(IEnumerable<Guid> ids)
    {
        int initialCount = _shows.Count;
            
        // Видалення всіх елементів, чий Id міститься у вхідному списку ідентифікаторів
        _shows.RemoveAll(s => ids.Contains(s.Id));
    
        return _shows.Count < initialCount;
    }
}