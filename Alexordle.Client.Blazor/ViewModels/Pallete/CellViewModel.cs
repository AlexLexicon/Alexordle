using Alexordle.Client.Application.Database.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alexordle.Client.Blazor.ViewModels.Pallete;
public partial class CellViewModel : ObservableObject
{
    [ObservableProperty]
    private int _width;

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private Hints _hint;

    [ObservableProperty]
    private bool _isHunch;

    public void Create(int width, char? character, Hints hint, bool isHunch)
    {
        Width = width;
        Text = character?.ToString();
        Hint = hint;
        IsHunch = isHunch;
    }
}
