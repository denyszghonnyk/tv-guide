using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TVProgram.Domain;

/// <summary>
/// Представляє телеканал, що містить розклад запланованих передач та керує ними.
/// </summary>
public record TVChannel
{
    private readonly List<TVShow> _shows = new();

    /// <summary>
    /// Унікальний глобальний ідентифікатор телеканалу (GUID).
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Назва телеканалу.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Список телепередач, закріплених за цим каналом.
    /// При ініціалізації список автоматично сортується за хронологічним порядком.
    /// </summary>
    public List<TVShow> Shows 
    { 
        get => _shows; 
        init 
        {
            _shows = value ?? new();
            _shows.Sort();
        }
    }

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="TVChannel"/> із значеннями за замовчуванням.
    /// </summary>
    public TVChannel() { }
       
    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="TVChannel"/> із заданими ідентифікатором, назвою та розкладом.
    /// Використовується для точної десеріалізації даних із JSON.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор каналу.</param>
    /// <param name="name">Назва телеканалу.</param>
    /// <param name="shows">Початковий список передач.</param>
    [JsonConstructor]
    public TVChannel(Guid id, string name, List<TVShow> shows)
    {
        Id = id;
        Name = name;
        Shows = shows;
    }
           
    /// <summary>
    /// Додає нову передачу до розкладу, перевіряючи її на відсутність часових конфліктів (колізій).
    /// </summary>
    /// <param name="newShow">Об'єкт передачі для додавання або редагування.</param>
    /// <returns>Об'єкт <see cref="Result{T}"/>, що містить оновлений канал у разі успіху або опис помилки колізії.</returns>
    public Result<TVChannel> AddShow(TVShow newShow)
    {
        if (string.IsNullOrWhiteSpace(newShow.Title))
        {
            return new Result<TVChannel>.Failure("Назва передачі не може бути порожньою.");
        }

        var conflict = Shows
            .Where(s => s.Id != newShow.Id) 
            .FirstOrDefault(existingShow => 
                newShow.StartTime < existingShow.EndTime && 
                existingShow.StartTime < newShow.EndTime);

        if (conflict is not null)
        {
            return new Result<TVChannel>.Failure(
                $"Конфлікт: '{newShow.Title}' перетинається з '{conflict.Title}'"
            );
        }

        var updatedShows = Shows
            .Where(s => s.Id != newShow.Id)
            .Append(newShow)
            .OrderBy(s => s.StartTime)
            .ToList();

        return new Result<TVChannel>.Success(this with { Shows = updatedShows });
    }

    /// <summary>
    /// Видаляє декілька передач із розкладу за їхніми унікальними ідентифікаторами.
    /// </summary>
    /// <param name="ids">Колекція ідентифікаторів передач, які необхідно видалити.</param>
    /// <returns>Новий імутабельний екземпляр <see cref="TVChannel"/> із оновленим розкладом.</returns>
    public TVChannel RemoveShows(IEnumerable<Guid> ids)
    {
        var updatedShows = Shows
            .Where(s => !ids.Contains(s.Id))
            .ToList();

        return this with { Shows = updatedShows };
    }
}