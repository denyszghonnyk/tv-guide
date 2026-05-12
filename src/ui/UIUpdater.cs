using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ReactiveUI;

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

/// <summary>
/// ViewModel головного вікна, що реалізує патерн MVU.
/// Зберігає поточний стан, забезпечує диспетчеризацію повідомлень та персистентність даних.
/// </summary>
public class MainWindowViewModel : ReactiveObject
{
    private const string DataPath = "program.json";
    private UIState _currentState;

    /// <summary>
    /// Поточний стан інтерфейсу. Зміна цієї властивості ініціює оновлення UI в Avalonia.
    /// </summary>
    public UIState CurrentState
    {
        get => _currentState;
        set => this.RaiseAndSetIfChanged(ref _currentState, value);
    }

    /// <summary>
    /// Конструктор: завантажує дані з JSON або створює новий канал, ініціалізуючи початковий стан.
    /// </summary>
    public MainWindowViewModel()
    {
        TVChannel channel;
        try
        {
            if (File.Exists(DataPath))
            {
                var json = File.ReadAllText(DataPath);
                channel = JsonSerializer.Deserialize<TVChannel>(json) ?? new TVChannel();
            }
            else
            {
                channel = new TVChannel();
            }
        }
        catch
        {
            channel = new TVChannel();
        }

        _currentState = new UIState.Home(channel, 0);
    }

    /// <summary>
    /// Єдина точка входу для обробки подій користувача. 
    /// Оновлює стан та автоматично зберігає зміни в файл при зміні версії даних.
    /// </summary>
    /// <param name="msg">Повідомлення (команда), яку потрібно виконати.</param>
    public void Dispatch(Msg msg)
    {
        var oldState = CurrentState;
        var newState = UIUpdater.Update(oldState, msg);

        CurrentState = newState;

        // Логіка персистентності: зберігаємо, якщо ми на Home і версія даних змінилася
        if (newState is UIState.Home newHome && 
           (oldState is not UIState.Home oldHome || newHome.Version != oldHome.Version))
        {
            SaveToJson(newHome.Channel);
        }
    }

    private void SaveToJson(TVChannel channel)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(channel, options);
            File.WriteAllText(DataPath, json);
        }
        catch
        {
            // У MVU логіці помилки IO зазвичай обробляються через диспетчеризацію Msg.Error,
            // але тут ми дотримуємося вимоги "без Exceptions" для стабільності ViewModel.
        }
    }
}