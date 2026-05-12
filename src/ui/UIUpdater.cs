namespace TVProgram.UI.Logic;

using TVProgram.UI.States;
using TVProgram.UI.Messages;
using TVProgram.Domain;

/// <summary>
/// Логіка оновлення стану інтерфейсу телепрограми.
/// </summary>
public static class UIUpdater
{
    /// <summary>
    /// Обробляє повідомлення та повертає новий стан на основі поточного.
    /// Реалізує Unidirectional Data Flow.
    /// </summary>
    /// <param name="currentState">Поточний стан системи.</param>
    /// <param name="msg">Повідомлення, що описує зміну.</param>
    /// <returns>Новий імутабельний стан UIState.</returns>
    public static UIState Update(UIState currentState, Msg msg) => (currentState, msg) switch
    {
        // Перехід до перегляду каналу
        (_, LoadChannel load) => new Home(load.Channel, 0),

        // Управління режимом вибору (Selection)
        (Home home, EnterSelectionMode) => new Selection(home.Channel, []),
        
        (Selection s, ToggleShowSelection toggle) => s with 
        { 
            // C# 13: Collection expressions та імутабельне оновлення HashSet
            SelectedIds = s.SelectedIds.Contains(toggle.Id) 
                ? [.. s.SelectedIds.Where(id => id != toggle.Id)] 
                : [.. s.SelectedIds, toggle.Id]
        },

        (Selection s, ConfirmDeletion) => 
            PerformDeletion(s.Channel, s.SelectedIds),

        // Редагування та створення
        (Home home, CreateNewShow) => new Editor(home.Channel, null),
        
        (Editor editor, SubmitSave saveMsg) => 
            PerformSave(editor.Channel, saveMsg.Show, editor),

        // Обробка помилок та скасування
        (Error error, DismissError) => error.PreviousState,
        
        (_, Cancel) or (_, DismissError) => currentState switch 
        {
            Editor e => new Home(e.Channel, 0),
            Selection s => new Home(s.Channel, 0),
            _ => currentState
        },

        // Default: якщо повідомлення не підтримується у поточному стані
        _ => currentState
    };

    /// <summary>
    /// Внутрішній метод для обробки видалення без винятків.
    /// </summary>
    private static UIState PerformDeletion(TVChannel channel, HashSet<Guid> ids)
    {
        channel.RemoveShows(ids);
        return new Home(channel, GetNextVersion(0)); // В реальному коді тут краще мати доступ до старої версії
    }

    /// <summary>
    /// Обробляє збереження передачі через Result ADT.
    /// </summary>
    private static UIState PerformSave(TVChannel channel, TVShow show, UIState previous)
    {
        Result<TVShow> result = channel.AddShow(show);

        return result switch
        {
            Result<TVShow>.Success => new Home(channel, GetNextVersion(0)),
            Result<TVShow>.Failure fail => new Error(fail.Message, previous),
            _ => new Error("Unknown result type", previous)
        };
    }

    /// <summary>
    /// Допоміжний метод для інкременту версії (забезпечує реактивність Avalonia).
    /// </summary>
    private static int GetNextVersion(int current) => current + 1;
}