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
    /// Додає нову передачу до списку, якщо вона не перетинається за часом з існуючими.
    /// </summary>
    /// <param name="newShow">Передача для додавання.</param>
    /// <returns>Result із доданою передачею або повідомленням про помилку.</returns>
    public Result<TVShow> AddShow(TVShow newShow)
    {
        // Перевірка умови перетину: (Start1 < End2) && (Start2 < End1)
        bool hasOverlap = _shows.Any(existingShow => 
            newShow.StartTime < existingShow.EndTime && 
            existingShow.StartTime < newShow.EndTime);

        if (hasOverlap)
        {
            return new Result<TVShow>.Failure("Час передачі перетинається з уже існуючою програмою.");
        }

        _shows.Add(newShow);
        _shows.Sort(); // Автоматичне сортування завдяки IComparable в TVShow

        return new Result<TVShow>.Success(newShow);
    }
}