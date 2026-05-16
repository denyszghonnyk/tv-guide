using TVProgram.Domain; 

namespace TVProgram.UI.ViewModels;

/// <summary>
/// Допоміжний клас-проекція (View Model Projection) для відображення телепередачі 
/// разом із її станом виділення (для списків з CheckBox).
/// Запобігає повному перемальовуванню списків Avalonia завдяки кастомній реалізації Equals.
/// </summary>
public class SelectableShow
{
    public TVShow Show { get; }
    public bool IsSelected { get; }

    public SelectableShow(TVShow show, bool isSelected)
    {
        Show = show;
        IsSelected = isSelected;
    }

    /// <summary>
    /// Порівнює об'єкти виключно за їх доменним ідентифікатором (Id).
    /// </summary>
    public override bool Equals(object? obj) => obj is SelectableShow other && Show.Id == other.Show.Id;
    public override int GetHashCode() => Show.Id.GetHashCode();
}