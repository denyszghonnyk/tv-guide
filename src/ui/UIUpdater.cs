using System;
using System.Collections.Generic;
using System.Linq;
using TVProgram.Domain;
using TVProgram.UI.States;
using TVProgram.UI.Messages;

namespace TVProgram.UI.Logic;
/// <summary>
/// Статичний клас, що містить чисту функцію оновлення стану (Update).
/// Відповідає за бізнес-логіку переходів між станами програми без побічних ефектів.
/// </summary>
public static class UIUpdater
{
    /// <summary>
    /// Головна функція переходу (Reducer). Приймає поточний стан та повідомлення, 
    /// повертаючи новий стан згідно з логікою застосунку.
    /// </summary>
    public static UIState Update(UIState currentState, Msg msg) => (currentState, msg) switch
    {
        // Перехід у режим вибору (Selection)
        (UIState.Home home, Msg.EnterSelectionMode) => 
            new UIState.Selection(home.Channel, []),

        // Перемикання вибору конкретного шоу (Імутабельне оновлення HashSet)
        (UIState.Selection sel, Msg.ToggleShowSelection(var id)) =>
            new UIState.Selection(sel.Channel, sel.SelectedIds.Contains(id) 
                ? sel.SelectedIds.Where(guid => guid != id).ToHashSet() 
                : [.. sel.SelectedIds, id]),

        // Підтвердження видалення вибраних елементів
        (UIState.Selection sel, Msg.ConfirmDeletion) =>
            ExecuteDeletion(sel),

        // Перехід до створення нового запису
        (UIState.Home home, Msg.CreateNewShow) => 
            new UIState.Editor(home.Channel, null),

        // Збереження результатів редагування/створення
        (UIState.Editor ed, Msg.SubmitSave(var show)) => 
            HandleSave(ed, show),

        // Відміна будь-якої дії та повернення на головний екран
        (_, Msg.Cancel) => 
            ToHome(currentState),

        // Закриття повідомлення про помилку
        (UIState.Error err, Msg.DismissError) => 
            err.PreviousState,

        // Обробка непередбачених станів (fallback)
        _ => currentState
    };

    private static UIState ExecuteDeletion(UIState.Selection state)
    {
        state.Channel.RemoveShows(state.SelectedIds);
        return new UIState.Home(state.Channel, GetNextVersion(state));
    }

    private static UIState HandleSave(UIState.Editor state, TVShow show)
    {
        return state.Channel.AddShow(show) switch
        {
            Result<TVShow>.Success => new UIState.Home(state.Channel, GetNextVersion(state)),
            Result<TVShow>.Failure(var msg) => new UIState.Error(msg, state),
            _ => state
        };
    }

    private static UIState.Home ToHome(UIState state) => state switch
    {
        UIState.Home h => h,
        UIState.Selection s => new UIState.Home(s.Channel, 0),
        UIState.Editor e => new UIState.Home(e.Channel, 0),
        UIState.Error err => ToHome(err.PreviousState),
        _ => throw new InvalidOperationException("Unknown state")
    };

    private static int GetNextVersion(UIState state) => state switch
    {
        UIState.Home h => h.Version + 1,
        UIState.Selection s => 1, // Базове скидання версії для спрощення
        UIState.Editor e => 1,
        _ => 0
    };
}