using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TVProgram.UI.States;

/// <summary>
/// Базовий абстрактний запис для всіх станів інтерфейсу телепрограми.
/// </summary>
public abstract record UIState;

/// <summary>
/// Стан головного екрана, де відображається список програм каналу.
/// </summary>
/// <param name="Channel">Поточний телеканал.</param>
/// <param name="Version">Версія даних, що інкрементується для примусового оновлення UI в Avalonia.</param>
public sealed record Home(TVChannel Channel, int Version) : UIState;

/// <summary>
/// Стан вибору програм для групових операцій (наприклад, видалення).
/// </summary>
/// <param name="Channel">Поточний телеканал.</param>
/// <param name="SelectedIds">Множина ідентифікаторів обраних програм.</param>
public sealed record Selection(TVChannel Channel, HashSet<Guid> SelectedIds) : UIState;

/// <summary>
/// Стан редагування або створення нової телепередачі.
/// </summary>
/// <param name="Channel">Поточний телеканал.</param>
/// <param name="ShowToEdit">Об'єкт передачі для редагування або null для створення нової.</param>
public sealed record Editor(TVChannel Channel, TVShow? ShowToEdit) : UIState;

/// <summary>
/// Стан відображення помилки, що виникла під час виконання операцій.
/// </summary>
/// <param name="Message">Текст помилки.</param>
/// <param name="PreviousState">Попередній стан для повернення після закриття повідомлення.</param>
public sealed record Error(string Message, UIState PreviousState) : UIState;