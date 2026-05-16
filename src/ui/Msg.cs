using System;
using TVProgram.Domain;

namespace TVProgram.UI.Messages;

/// <summary>
/// Базовий абстрактний запис для всіх повідомлень (інтентів), що змінюють глобальний стан системи.
/// </summary>
internal abstract record Msg
{
    /// <summary>Перевести інтерфейс у режим вибору елементів для групового видалення.</summary>
    internal sealed record EnterSelectionMode : Msg;

    /// <summary>Перемкнути стан виділення для конкретної передачі за її ідентифікатором.</summary>
    /// <param name="Id">Унікальний ідентифікатор телепередачі.</param>
    internal sealed record ToggleShowSelection(Guid Id) : Msg;

    /// <summary>Підтвердити видалення всіх обраних елементів розкладу.</summary>
    internal sealed record ConfirmDeletion : Msg;

    /// <summary>Скасувати поточну дію (редагування чи вибір) та повернутися на головний екран.</summary>
    internal sealed record Cancel : Msg;

    /// <summary>Відкрити форму редактора для створення нової телепередачі.</summary>
    internal sealed record CreateNewShow : Msg;

    /// <summary>Закрити інформаційне вікно помилки та повернутися до попереднього стану.</summary>
    internal sealed record DismissError : Msg;

    /// <summary>Ініціювати відображення повідомлення про помилку з відповідним текстом.</summary>
    /// <param name="Message">Текст помилки для відображення користувачу.</param>
    internal sealed record ShowError(string Message) : Msg;

    /// <summary>Перемкнути поточний активний телеканал на головному екрані.</summary>
    /// <param name="ChannelId">Ідентифікатор цільового телеканалу.</param>
    internal sealed record SwitchChannel(Guid ChannelId) : Msg;

    /// <summary>Завантажити повну структуру даних (кореневий агрегат) та встановити активний канал.</summary>
    /// <param name="Root">Кореневий об'єкт телепрограми.</param>
    /// <param name="ActiveChannelId">Ідентифікатор каналу, який має стати активним.</param>
    internal sealed record LoadData(TVGuideRoot Root, Guid ActiveChannelId) : Msg;

    /// <summary>Перемкнути статус знаходження передачі в персональному списку обраного (Watchlist).</summary>
    /// <param name="ShowId">Ідентифікатор телепередачі.</param>
    internal sealed record ToggleWatchlist(Guid ShowId) : Msg;

    /// <summary>Відкрити форму редактора для зміни існуючої телепередачі.</summary>
    /// <param name="ChannelId">Ідентифікатор каналу, якому належить передача.</param>
    /// <param name="Show">Об'єкт передачі, яку необхідно відредагувати.</param>
    internal sealed record EditShow(Guid ChannelId, TVShow Show) : Msg;

    /// <summary>Підтвердити та зберегти результати редагування або створення передачі у цільовий канал.</summary>
    /// <param name="TargetChannelId">Ідентифікатор каналу, куди буде збережено передачу.</param>
    /// <param name="Show">Об'єкт телепередачі з новими даними.</param>
    internal sealed record SubmitSave(Guid TargetChannelId, TVShow Show) : Msg;

    /// <summary>Створити новий телеканал із заданою назвою прямо з форми редагування.</summary>
    /// <param name="Name">Назва нової телекомпанії.</param>
    internal sealed record AddNewChannel(string Name) : Msg;
}