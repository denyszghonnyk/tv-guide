namespace TVProgram.UI.Messages;

using TVProgram.Domain;

/// <summary>
/// Базовий запис для всіх повідомлень (інтентів), що змінюють стан системи.
/// </summary>
public abstract record Msg;

/// <summary>Завантажити дані каналу.</summary>
public sealed record LoadChannel(TVChannel Channel) : Msg;

/// <summary>Перейти в режим вибору елементів.</summary>
public sealed record EnterSelectionMode : Msg;

/// <summary>Перемкнути стан вибору для конкретної передачі.</summary>
public sealed record ToggleShowSelection(Guid Id) : Msg;

/// <summary>Підтвердити видалення обраних елементів.</summary>
public sealed record ConfirmDeletion : Msg;

/// <summary>Скасувати поточну дію та повернутися на головний екран.</summary>
public sealed record Cancel : Msg;

/// <summary>Розпочати створення нової передачі.</summary>
public sealed record CreateNewShow : Msg;

/// <summary>Зберегти зміни у передачі.</summary>
public sealed record SubmitSave(TVShow Show) : Msg;

/// <summary>Закрити повідомлення про помилку.</summary>
public sealed record DismissError : Msg;