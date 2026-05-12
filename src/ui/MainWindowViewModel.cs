using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TVProgram.Domain; // Переконайся, що тут твій неймспейс домену
using TVProgram.UI.Logic; // Неймспейс для UIUpdater
using TVProgram.UI.States; // Неймспейс для UIState
using TVProgram.UI.Messages; // Неймспейс для Msg

namespace TVProgram.UI.ViewModels;

/// <summary>
/// ViewModel, що виступає мостом між імутабельним MVU ядром та Avalonia UI.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private const string FileName = "tv_program.json";

    [ObservableProperty]
    private UIState _currentState;

    // Draft-властивості для форми редагування
    [ObservableProperty] private string _draftTitle = string.Empty;
    [ObservableProperty] private DateTimeOffset _draftStartTime = DateTimeOffset.Now;
    [ObservableProperty] private TimeSpan _draftDuration = TimeSpan.FromMinutes(30);

    public MainWindowViewModel()
    {
        // Запускаємо стартовий стан (можна додати завантаження з файлу тут)
        _currentState = new UIState.Home(new TVChannel(), 0);
    }

    #region Commands
    
    [RelayCommand]
    private void LoadChannel() => Dispatch(new Msg.LoadChannel(new TVChannel()));

    [RelayCommand]
    private void EnterSelectionMode() => Dispatch(new Msg.EnterSelectionMode());

    [RelayCommand]
    private void ToggleSelection(Guid id) => Dispatch(new Msg.ToggleShowSelection(id));

    [RelayCommand]
    private void ConfirmDeletion() => Dispatch(new Msg.ConfirmDeletion());

    [RelayCommand]
    private void CreateNewShow() => Dispatch(new Msg.CreateNewShow());

    [RelayCommand]
    private void Cancel() => Dispatch(new Msg.Cancel());

    [RelayCommand]
    private void DismissError() => Dispatch(new Msg.DismissError());

    [RelayCommand]
    private void Save()
    {
        if (CurrentState is UIState.Editor editor)
        {
            var id = editor.ShowToEdit?.Id ?? Guid.NewGuid();
            // Створюємо новий імутабельний об'єкт (жанр ставимо за замовчуванням, оскільки його немає в UI)
            var newShow = new TVShow(id, DraftTitle, DraftStartTime.DateTime, DraftDuration, Genre.News); 
            Dispatch(new Msg.SubmitSave(newShow));
        }
    }

    #endregion

    /// <summary>
    /// Єдина точка мутації стану через чисту функцію.
    /// </summary>
    private void Dispatch(Msg msg)
    {
        CurrentState = UIUpdater.Update(CurrentState, msg);
        HandleStateChange(CurrentState);
    }

    private void HandleStateChange(UIState state)
    {
        if (state is UIState.Editor editor)
        {
            if (editor.ShowToEdit != null)
            {
                DraftTitle = editor.ShowToEdit.Title;
                DraftStartTime = new DateTimeOffset(editor.ShowToEdit.StartTime);
                DraftDuration = editor.ShowToEdit.Duration;
            }
            else
            {
                DraftTitle = string.Empty;
                DraftStartTime = DateTimeOffset.Now;
                DraftDuration = TimeSpan.FromMinutes(30);
            }
        }
        else if (state is UIState.Home home)
        {
            try
            {
                var json = JsonSerializer.Serialize(home.Channel, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FileName, json);
            }
            catch { /* Ігноруємо помилки доступу до файлу для стабільності */ }
        }
    }
}