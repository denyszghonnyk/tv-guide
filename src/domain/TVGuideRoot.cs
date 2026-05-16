using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TVProgram.Domain;

/// <summary>
/// Кореневий агрегат домену (Root Aggregate) застосунку, що об'єднує всі доступні телеканали 
/// та глобальні дані планувальника користувача, забезпечуючи цілісність даних (Data Integrity).
/// </summary>
public record TVGuideRoot
{
    /// <summary>
    /// Список усіх телеканалів (телекомпаній), зареєстрованих у системі.
    /// </summary>
    public List<TVChannel> Channels { get; init; } = new();

    /// <summary>
    /// Множина унікальних ідентифікаторів (GUID) телепередач, які користувач додав до персонального списку "Вибране" (планувальника).
    /// </summary>
    public HashSet<Guid> Watchlist { get; init; } = new();

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="TVGuideRoot"/> із значеннями за замовчуванням.
    /// </summary>
    public TVGuideRoot() { }

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="TVGuideRoot"/> із заданими списками каналів та обраного.
    /// Помічений атрибутом <see cref="JsonConstructorAttribute"/> для коректного відновлення стану агрегата з JSON.
    /// </summary>
    /// <param name="channels">Початковий список телеканалів.</param>
    /// <param name="watchlist">Початкова множина ідентифікаторів обраних передач.</param>
    [JsonConstructor]
    public TVGuideRoot(List<TVChannel> channels, HashSet<Guid> watchlist)
    {
        Channels = channels ?? new();
        Watchlist = watchlist ?? new();
    }

    /// <summary>
    /// Чиста функція (Pure Function), яка перемикає статус знаходження передачі в списку обраного (додає або видаляє).
    /// Реалізує принцип незмінності об'єктів (Immutability).
    /// </summary>
    /// <param name="showId">Унікальний ідентифікатор телепередачі.</param>
    /// <returns>Новий імутабельний екземпляр <see cref="TVGuideRoot"/> із оновленим планувальником.</returns>
    public TVGuideRoot ToggleWatchlist(Guid showId)
    {
        var newWatchlist = new HashSet<Guid>(Watchlist);
        if (!newWatchlist.Add(showId))
        {
            newWatchlist.Remove(showId);
        }
        
        return this with { Watchlist = newWatchlist };
    }

    /// <summary>
    /// Чиста функція (Pure Function) для імутабельного оновлення даних конкретного телеканалу в загальному списку розкладу.
    /// </summary>
    /// <param name="updatedChannel">Об'єкт телеканалу з оновленим списком передач.</param>
    /// <returns>Новий імутабельний екземпляр <see cref="TVGuideRoot"/> із оновленою колекцією каналів.</returns>
    public TVGuideRoot UpdateChannel(TVChannel updatedChannel)
    {
        var newChannels = Channels
            .Where(c => c.Id != updatedChannel.Id)
            .Append(updatedChannel)
            .ToList();
            
        return this with { Channels = newChannels };
    }
}