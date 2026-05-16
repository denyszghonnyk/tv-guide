using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TVProgram.Domain;

/// <summary>
/// Кореневий агрегат застосунку (містить усі канали та налаштування користувача).
/// </summary>
internal sealed record TVGuideRoot
{
    // Список усіх телекомпаній
    public List<TVChannel> Channels { get; init; } = new();

    // Планувальник: зберігаємо лише Id передач, які користувач додав у "Вибране"
    public HashSet<Guid> Watchlist { get; init; } = new();

    public TVGuideRoot() { }

    [JsonConstructor]
    public TVGuideRoot(List<TVChannel> channels, HashSet<Guid> watchlist)
    {
        Channels = channels ?? new();
        Watchlist = watchlist ?? new();
    }

    /// <summary>
    /// Чиста функція для перемикання статусу "Обране".
    /// </summary>
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
    /// Оновлює конкретний канал у списку і повертає новий корінь.
    /// </summary>
    public TVGuideRoot UpdateChannel(TVChannel updatedChannel)
    {
        var newChannels = Channels
            .Where(c => c.Id != updatedChannel.Id)
            .Append(updatedChannel)
            .ToList();

        return this with { Channels = newChannels };
    }
}