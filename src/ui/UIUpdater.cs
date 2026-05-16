using System;
using System.Collections.Generic;
using System.Linq;
using TVProgram.Domain;
using TVProgram.UI.States;
using TVProgram.UI.Messages;

namespace TVProgram.UI.Logic;

/// <summary>
/// Статичний клас, що відповідає за центральну логіку керування станом (State Management).
/// Реалізує чисту функцію оновлення (Update) в архітектурному патерні MVU (Model-View-Update).
/// </summary>
public static class UIUpdater
{
    /// <summary>
    /// Головна чиста функція (Pure Function) застосунку, яка приймає поточний стан системи та повідомлення, 
    /// обробляє його за допомогою Pattern Matching і повертає новий детермінований стан.
    /// </summary>
    /// <param name="currentState">Поточний імутабельний стан користувацького інтерфейсу.</param>
    /// <param name="msg">Об'єкт повідомлення (інтент), що описує подію або дію користувача.</param>
    /// <returns>Новий об'єкт стану системи <see cref="UIState"/>, на основі якого Avalonia перебудує UI.</returns>
    public static UIState Update(UIState currentState, Msg msg) => (currentState, msg) switch
    {
        // Перехід у режим вибору
        (UIState.Home home, Msg.EnterSelectionMode) => 
            new UIState.Selection(home.RootData, home.ActiveChannelId, []),

        // Завантаження нових даних
        (UIState.Home _, Msg.LoadData(var root, var activeId)) =>
            new UIState.Home(root, activeId, 1),
                
        // Перемикання вибору для видалення
        (UIState.Selection sel, Msg.ToggleShowSelection(var id)) =>
            new UIState.Selection(sel.RootData, sel.ActiveChannelId, sel.SelectedIds.Contains(id) 
                ? sel.SelectedIds.Where(guid => guid != id).ToHashSet() 
                : [.. sel.SelectedIds, id]),

        // Підтвердження видалення
        (UIState.Selection sel, Msg.ConfirmDeletion) => HandleDeletion(sel),
        
        // Створення нового каналу з редактора
        (UIState.Editor ed, Msg.AddNewChannel addMsg) => HandleAddChannel(ed, addMsg.Name),
        
        // Відкриття редактора для нової передачі
        (UIState.Home home, Msg.CreateNewShow) => 
            new UIState.Editor(home.RootData, home.ActiveChannelId, home.ActiveChannelId, null),

        // Відкриття редактора для існуючої
        (UIState.Home home, Msg.EditShow edit) => 
            new UIState.Editor(home.RootData, home.ActiveChannelId, edit.ChannelId, edit.Show),

        // Збереження
        (UIState.Editor ed, Msg.SubmitSave save) => HandleSave(ed, save.TargetChannelId, save.Show),

        // Додавання/видалення з обраного (Watchlist)
        (UIState.Home home, Msg.ToggleWatchlist(var id)) =>
            new UIState.Home(home.RootData.ToggleWatchlist(id), home.ActiveChannelId, GetNextVersion(home)),

        // Перемикання каналів через вкладки
        (UIState.Home home, Msg.SwitchChannel(var newChannelId)) =>
            new UIState.Home(home.RootData, newChannelId, GetNextVersion(home)),

        // Відміна та помилки
        (_, Msg.Cancel) => ToHome(currentState),
        (UIState.Error err, Msg.DismissError) => err.PreviousState,
        (UIState state, Msg.ShowError err) => new UIState.Error(err.Message, state),

        _ => currentState
    };

    // Обробка видалення вибраних передач з активного каналу
    private static UIState HandleDeletion(UIState.Selection sel)
    {
        var activeChannel = sel.RootData.Channels.FirstOrDefault(c => c.Id == sel.ActiveChannelId);
        if (activeChannel == null) return new UIState.Home(sel.RootData, sel.ActiveChannelId, 1);

        var updatedChannel = activeChannel.RemoveShows(sel.SelectedIds);
        return new UIState.Home(sel.RootData.UpdateChannel(updatedChannel), sel.ActiveChannelId, 1);
    }

    // Обробка збереження передачі з урахуванням зміни каналу та перевірки на часові колізії
    private static UIState HandleSave(UIState.Editor state, Guid targetChannelId, TVShow show)
    {
        var root = state.RootData;

        // 1. Якщо канал змінили — видаляємо передачу зі старого каналу
        if (state.SourceChannelId.HasValue && state.SourceChannelId.Value != targetChannelId)
        {
            if (root.Channels.FirstOrDefault(c => c.Id == state.SourceChannelId.Value) is { } source)
            {
                root = root.UpdateChannel(source.RemoveShows([show.Id]));
            }
        }

        // 2. Додаємо в новий цільовий канал
        var targetChannel = root.Channels.FirstOrDefault(c => c.Id == targetChannelId);
        if (targetChannel == null) return state;

        return targetChannel.AddShow(show) switch
        {
            Result<TVChannel>.Success(var newChannel) => 
                new UIState.Home(root.UpdateChannel(newChannel), targetChannelId, GetNextVersion(state)),
            
            Result<TVChannel>.Failure(var msg) => 
                new UIState.Error(msg, state),
                
            _ => state
        };
    }

    // Допоміжний метод для безпечного повернення до стану Home з будь-котрого екрана
    private static UIState.Home ToHome(UIState state) => state switch
    {
        UIState.Home h => h,
        UIState.Selection s => new UIState.Home(s.RootData, s.ActiveChannelId, 0),
        UIState.Editor e => new UIState.Home(e.RootData, e.ActiveChannelId, 0),
        UIState.Error err => ToHome(err.PreviousState),
        _ => throw new InvalidOperationException("Unknown state")
    };

    // Обчислює наступну версію стану для примусового оновлення відображення в Avalonia UI
    private static int GetNextVersion(UIState state) => state switch
    {
        UIState.Home h => h.Version + 1,
        UIState.Selection s => 1,
        UIState.Editor e => 1,
        _ => 0   
    };
    
    // Створення нового телеканалу та автоматичний вибір його у формі редактора
    private static UIState HandleAddChannel(UIState.Editor state, string name)
    {
        var newChannel = new TVChannel { Id = Guid.NewGuid(), Name = name, Shows = new List<TVShow>() };
        var newChannels = state.RootData.Channels.Append(newChannel).ToList();
        var newRoot = state.RootData with { Channels = newChannels };
        
        return new UIState.Editor(newRoot, state.ActiveChannelId, newChannel.Id, state.ShowToEdit);
    }
}