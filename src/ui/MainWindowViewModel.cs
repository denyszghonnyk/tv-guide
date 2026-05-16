using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TVProgram.Domain;
using TVProgram.UI.Logic;
using TVProgram.UI.States;
using TVProgram.UI.Messages;
using System.Text.Json.Serialization;

namespace TVProgram.UI.ViewModels;

/// <summary>
/// Головна View Model застосунку. 
/// Виступає мостом між імутабельним ядром (MVU State) та реактивним інтерфейсом Avalonia (Data Binding).
/// </summary>
internal partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string CurrentFilePath { get; set; } = "tv_program.json";

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow?.StorageProvider;
        return null;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(), new TimeSpanConverter() }
    };

    public static IEnumerable<Genre> AvailableGenres => Enum.GetValues<Genre>();

    [ObservableProperty]
    public partial Genre DraftGenre { get; set; } = Genre.Новини;

    [ObservableProperty]
    public partial UIState CurrentState { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredShows))]
    public partial string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Обчислювана властивість, що повертає список передач для поточного екрана.
    /// Враховує обраний канал, рядок пошуку та фільтр "Тільки обране".
    /// </summary>
    public IEnumerable<TVShow> FilteredShows
    {
        get
        {
            TVGuideRoot? root = null;
            Guid? activeId = null;

            if (CurrentState is UIState.Home home) { root = home.RootData; activeId = home.ActiveChannelId; }
            else if (CurrentState is UIState.Selection sel) { root = sel.RootData; activeId = sel.ActiveChannelId; }

            if (root != null && activeId != null)
            {
                var activeChannel = root.Channels.FirstOrDefault(c => c.Id == activeId);
                if (activeChannel == null) return Enumerable.Empty<TVShow>();

                var baseList = activeChannel.Shows.AsEnumerable();

                if (ShowOnlyWatchlist)
                    baseList = baseList.Where(s => root.Watchlist.Contains(s.Id));

                if (string.IsNullOrWhiteSpace(SearchQuery)) return baseList.ToList();

                return baseList.Where(s => s.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return Enumerable.Empty<TVShow>();
        }
    }

    /// <summary>
    /// Поточний обраний телеканал на головному екрані.
    /// Зміна цієї властивості ініціює подію зміни каналу.
    /// </summary>
    public TVChannel? SelectedChannel
    {
        get
        {
            if (CurrentState is UIState.Home home)
                return home.RootData.Channels.FirstOrDefault(c => c.Id == home.ActiveChannelId);
            if (CurrentState is UIState.Selection sel)
                return sel.RootData.Channels.FirstOrDefault(c => c.Id == sel.ActiveChannelId);
            return null;
        }
        set
        {
            if (value != null)
            {
                Dispatch(new Msg.SwitchChannel(value.Id));
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredShows))]
    public partial bool ShowOnlyWatchlist { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial TVChannel? DraftChannel { get; set; }

    /// <summary>
    /// Список доступних телеканалів для випадаючого списку (ComboBox) у редакторі.
    /// </summary>
    public IEnumerable<TVChannel> AvailableChannels =>
        (CurrentState as UIState.Editor)?.RootData.Channels ?? Enumerable.Empty<TVChannel>();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string DraftTitle { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DraftStartTimeOfDay))]
    public partial DateTimeOffset DraftStartTime { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// Обгортка (Wrapper) над часом початку для сумісності з компонентом TimePicker (Avalonia UI).
    /// </summary>
    public TimeSpan? DraftStartTimeOfDay
    {
        get => DraftStartTime.TimeOfDay;
        set
        {
            if (value.HasValue)
            {
                var date = DraftStartTime.Date;
                DraftStartTime = new DateTimeOffset(
                    date.Year, date.Month, date.Day,
                    value.Value.Hours, value.Value.Minutes, value.Value.Seconds,
                    DraftStartTime.Offset);
                OnPropertyChanged();
            }
        }
    }

    private TimeSpan _draftDuration = TimeSpan.FromMinutes(30);

    public TimeSpan DraftDuration
    {
        get => _draftDuration;
        set
        {
            _draftDuration = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DraftDurationMinutes));
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Обгортка (Wrapper) над тривалістю передачі у хвилинах для NumericUpDown.
    /// </summary>
    public decimal? DraftDurationMinutes
    {
        get => (decimal)_draftDuration.TotalMinutes;
        set => DraftDuration = TimeSpan.FromMinutes((double)(value ?? 1));
    }

    [ObservableProperty]
    public partial bool IsAddingChannel { get; set; }

    [ObservableProperty]
    public partial string NewChannelName { get; set; } = string.Empty;

    [RelayCommand]
    private void ToggleAddChannel()
    {
        IsAddingChannel = !IsAddingChannel;
        NewChannelName = string.Empty;
    }

    [RelayCommand]
    private void SubmitNewChannel()
    {
        if (!string.IsNullOrWhiteSpace(NewChannelName))
        {
            Dispatch(new Msg.AddNewChannel(NewChannelName.Trim()));
            IsAddingChannel = false;
            NewChannelName = string.Empty;
        }
    }

    /// <summary>
    /// Конструктор View Model. Створює початковий базовий стан із дефолтним каналом та намагається завантажити дані.
    /// </summary>
    public MainWindowViewModel()
    {
        var defaultChannel = new TVChannel { Name = "Основний канал" };
        var root = new TVGuideRoot(new List<TVChannel> { defaultChannel }, new HashSet<Guid>());
        CurrentState = new UIState.Home(root, defaultChannel.Id, 0);

        LoadChannel();
    }

    #region Commands

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (CurrentState is UIState.Editor editor && DraftChannel != null)
        {
            var id = editor.ShowToEdit?.Id ?? Guid.NewGuid();
            var newShow = new TVShow(id, DraftTitle, DraftStartTime.DateTime, DraftDuration, DraftGenre);
            Dispatch(new Msg.SubmitSave(DraftChannel.Id, newShow));
        }
    }

    private bool CanSave() =>
            !string.IsNullOrWhiteSpace(DraftTitle) && DraftDuration.TotalMinutes > 0 && DraftChannel != null;

    [RelayCommand]
    private async Task OpenFile()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;

        var options = new FilePickerOpenOptions
        {
            Title = "Виберіть файл телепрограми",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } }
        };

        try
        {
            var result = await sp.OpenFilePickerAsync(options);
            if (result.Count > 0)
            {
                var file = result[0];
                CurrentFilePath = file.Path.LocalPath;
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                ProcessLoadedJson(json); 
            }
        }
        catch (Exception) { Dispatch(new Msg.ShowError("Помилка при відкритті файлу.")); }
    }

    [RelayCommand]
    private void LoadChannel()
    {
        if (!File.Exists(CurrentFilePath)) return;
        try
        {
            var json = File.ReadAllText(CurrentFilePath);
            if (string.IsNullOrWhiteSpace(json)) return;
            ProcessLoadedJson(json);
        }
        catch (Exception ex) { Console.WriteLine($"Load error: {ex.Message}"); }
    }

    private void ProcessLoadedJson(string json)
    {
        try
        {
            if (JsonSerializer.Deserialize<TVGuideRoot>(json, _jsonOptions) is { } root && root.Channels != null)
            {
                var activeId = root.Channels.FirstOrDefault()?.Id ?? Guid.NewGuid();
                Dispatch(new Msg.LoadData(root, activeId));
                return;
            }
        }
        catch (JsonException)
        {
            try
            {
                if (JsonSerializer.Deserialize<TVChannel>(json, _jsonOptions) is { } oldChannel)
                {
                    var root = new TVGuideRoot(new List<TVChannel> { oldChannel }, new HashSet<Guid>());
                    Dispatch(new Msg.LoadData(root, oldChannel.Id));
                    return;
                }
            }
            catch (JsonException)
            {
                Dispatch(new Msg.ShowError("Файл має невірний формат. Будь ласка, переконайтеся, що ви відкриваєте збережену телепрограму."));
                return;
            }
        }

        Dispatch(new Msg.ShowError("Файл порожній або містить пошкоджені дані."));
    }

    [RelayCommand]
    private async Task SaveAs()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;

        var options = new FilePickerSaveOptions { Title = "Зберегти як...", DefaultExtension = "json" };
        try
        {
            var file = await sp.SaveFilePickerAsync(options).ConfigureAwait(false);
            if (file != null)
            {
                CurrentFilePath = file.Path.LocalPath;
                await SaveToFile().ConfigureAwait(false);
            }
        }
        catch (Exception ex) { Dispatch(new Msg.ShowError(ex.Message)); }
    }

    [RelayCommand]
    private async Task SaveToFile()
    {
        if (CurrentState is UIState.Home home)
        {
            try
            {
                var json = JsonSerializer.Serialize(home.RootData, _jsonOptions);
                await File.WriteAllTextAsync(CurrentFilePath, json).ConfigureAwait(false);
            }
            catch (Exception ex) { Dispatch(new Msg.ShowError(ex.Message)); }
        }
    }

    [RelayCommand]
    private void EnterSelectionMode() => Dispatch(new Msg.EnterSelectionMode());

    [RelayCommand]
    private void ToggleSelection(Guid id) => Dispatch(new Msg.ToggleShowSelection(id));

    [RelayCommand]
    private void ConfirmDeletion() => Dispatch(new Msg.ConfirmDeletion());

    [RelayCommand]
    private void CreateNewShow() => Dispatch(new Msg.CreateNewShow());

    [RelayCommand]
    private void EditShow(Guid id)
    {
        if (CurrentState is UIState.Home home)
        {
            foreach (var channel in home.RootData.Channels)
            {
                if (channel.Shows.FirstOrDefault(s => s.Id == id) is { } show)
                {
                    Dispatch(new Msg.EditShow(channel.Id, show));
                    return;
                }
            }
        }
    }

    [RelayCommand]
    private void ToggleWatchlist(Guid id) => Dispatch(new Msg.ToggleWatchlist(id));

    [RelayCommand]
    private void Cancel() => Dispatch(new Msg.Cancel());

    [RelayCommand]
    private void DismissError() => Dispatch(new Msg.DismissError());

    #endregion

    /// <summary>
    /// Проекція (Projection) моделі для UI.
    /// Перетворює доменні сутності <see cref="TVShow"/> на обгортки <see cref="SelectableShow"/> 
    /// зі збереженням статусу виділення (CheckBox) для коректного Data Binding в Avalonia.
    /// </summary>
    public IEnumerable<SelectableShow> SelectionShows
    {
        get
        {
            if (CurrentState is UIState.Selection sel)
            {
                return FilteredShows.Select(s => new SelectableShow(s, sel.SelectedIds.Contains(s.Id))).ToList();
            }
            return Enumerable.Empty<SelectableShow>();
        }
    }

    /// <summary>
    /// Єдина точка входу для зміни стану (State Management).
    /// Приймає повідомлення (Msg), передає його чистій функції оновлення та повідомляє UI про зміни.
    /// </summary>
    private void Dispatch(Msg msg)
    {
        CurrentState = UIUpdater.Update(CurrentState, msg);

        OnPropertyChanged(nameof(FilteredShows));
        OnPropertyChanged(nameof(SelectionShows));

        HandleStateChange(CurrentState);
    }

    /// <summary>
    /// Синхронізує поля форми-редактора з поточним глобальним станом після його оновлення.
    /// </summary>
    private void HandleStateChange(UIState state)
    {
        if (state is UIState.Editor editor)
        {
            OnPropertyChanged(nameof(AvailableChannels));

            DraftChannel = editor.RootData.Channels.FirstOrDefault(c => c.Id == editor.SourceChannelId);

            if (editor.ShowToEdit != null)
            {
                DraftTitle = editor.ShowToEdit.Title;
                DraftStartTime = new DateTimeOffset(editor.ShowToEdit.StartTime);
                DraftDuration = editor.ShowToEdit.Duration;
                DraftGenre = editor.ShowToEdit.Genre;
            }
            else
            {
                DraftTitle = string.Empty;
                DraftStartTime = DateTimeOffset.Now;
                DraftDuration = TimeSpan.FromMinutes(30);
                DraftGenre = Genre.Новини;
            }
        }
    }
}