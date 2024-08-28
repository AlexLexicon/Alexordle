using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Mvvm;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Keyboard;
public partial class KeyboardViewModel : ObservableObject
{
    public const string ENTER_KEY = "Enter";
    public const string BACKSPACE_KEY = "⌫";

    private const string ROW_1 = "q,w,e,r,t,y,u,i,o,p";
    private const string ROW_2 = "a,s,d,f,g,h,j,k,l";
    private const string ROW_3 = $"{BACKSPACE_KEY},z,x,c,v,b,n,m,{ENTER_KEY}";

    private readonly IViewModelFactory _viewModelFactory;

    public KeyboardViewModel(IViewModelFactory viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;

        Row1KeyViewModels = [];
        Row2KeyViewModels = [];
        Row3KeyViewModels = [];
    }

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row1KeyViewModels;

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row2KeyViewModels;

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row3KeyViewModels;

    public void Create()
    {
        Row1KeyViewModels.Clear();
        Row2KeyViewModels.Clear();
        Row3KeyViewModels.Clear();

        CreateKeys(ROW_1, Row1KeyViewModels);
        CreateKeys(ROW_2, Row2KeyViewModels);
        CreateKeys(ROW_3, Row3KeyViewModels, widen: true);
    }

    private void CreateKeys(string allKeysString, ObservableCollection<KeyViewModel> keyViewModels, bool widen = false)
    {
        string[] keys = allKeysString.Split(',');

        for (int i = 0; i < keys.Length; i++)
        {
            string text = keys[i];

            bool isWide = widen && (i is 0 || i == keys.Length - 1);

            var keyViewModel = _viewModelFactory.Create<KeyViewModel>();

            char? invariantCharacter = text.Length is 1 ? char.ToUpperInvariant(text[0]) : null;

            keyViewModel.Create(text, isWide, invariantCharacter);

            keyViewModels.Add(keyViewModel);
        }
    }
}
