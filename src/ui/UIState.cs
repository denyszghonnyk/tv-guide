using System;
using System.Collections.Generic;
using TVProgram.Domain;

namespace TVProgram.UI.States;

/// <summary>
/// Базовий абстрактний запис для всіх станів інтерфейсу (UI States) телепрограми.
/// Використовується для реалізації патерну State Machine в архітектурі MVU.
/// </summary>
public abstract record UIState
{
    /// <summary>
    /// Стан головного екрана застосунку, де відображається розклад розширеної телепрограми.
    /// </summary>
    /// <param name="RootData">Кореневий агрегат даних, що містить усі канали та списки фільтрації.</param>
    /// <param name="ActiveChannelId">Унікальний ідентифікатор поточного активного каналу, розклад якого відображається.</param>
    /// <param name="Version">Версія даних, що інкрементується для примусового оновлення реактивного UI в Avalonia.</param>
    public sealed record Home(TVGuideRoot RootData, Guid ActiveChannelId, int Version) : UIState;

    /// <summary>
    /// Стан вибору телепередач для виконання групових операцій (наприклад, множинного видалення з розкладу).
    /// </summary>
    /// <param name="RootData">Кореневий агрегат даних телепрограми.</param>
    /// <param name="ActiveChannelId">Унікальний ідентифікатор поточного активного каналу.</param>
    /// <param name="SelectedIds">Множина унікальних ідентифікаторів (GUID) передач, які було відмічено користувачем.</param>
    public sealed record Selection(TVGuideRoot RootData, Guid ActiveChannelId, HashSet<Guid> SelectedIds) : UIState;

    /// <summary>
    /// Стан відображення критичної помилки або інформаційного попередження у користувацькому інтерфейсі.
    /// </summary>
    /// <param name="Message">Текст повідомлення про помилку, що виводиться на екран.</param>
    /// <param name="PreviousState">Попередній стан системи для відновлення контексту роботи після закриття повідомлення.</param>
    public sealed record Error(string Message, UIState PreviousState) : UIState;
    
    /// <summary>
    /// Стан форми створення нової телепередачі або редагування параметрів уже існуючої.
    /// </summary>
    /// <param name="RootData">Кореневий агрегат даних телепрограми.</param>
    /// <param name="ActiveChannelId">Унікальний ідентифікатор поточного активного каналу.</param>
    /// <param name="SourceChannelId">Ідентифікатор початкового каналу передачі (використовується для логіки зміни каналу в формі).</param>
    /// <param name="ShowToEdit">Об'єкт телепередачі для редагування або null, якщо створюється нова передача з нуля.</param>
    public sealed record Editor(TVGuideRoot RootData, Guid ActiveChannelId, Guid? SourceChannelId, TVShow? ShowToEdit) : UIState;
}