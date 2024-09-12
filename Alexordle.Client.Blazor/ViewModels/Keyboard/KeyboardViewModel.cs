using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Notifications;
using Lexicom.Mvvm;
using MediatR;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Keyboard;
public partial class KeyboardViewModel : ObservableObject, INotificationHandler<KeyboardKeyPressNotification>
{
    private const string DELIMITER = ",";
    public const string ENTER_KEY = "ENTER";
    public const string BACKSPACE_KEY = "⌫";

    private const string ROW_1 = $"Q{DELIMITER}W{DELIMITER}E{DELIMITER}R{DELIMITER}T{DELIMITER}Y{DELIMITER}U{DELIMITER}I{DELIMITER}O{DELIMITER}P";
    private const string ROW_2 = $"A{DELIMITER}S{DELIMITER}D{DELIMITER}F{DELIMITER}G{DELIMITER}H{DELIMITER}J{DELIMITER}K{DELIMITER}L";
    private const string ROW_3 = $"Z{DELIMITER}X{DELIMITER}C{DELIMITER}V{DELIMITER}B{DELIMITER}N{DELIMITER}M";

    private readonly IMediator _mediator;
    private readonly IViewModelFactory _viewModelFactory;

    public KeyboardViewModel(
        IMediator mediator,
        IViewModelFactory viewModelFactory)
    {
        _mediator = mediator;
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

    public async Task Handle(KeyboardKeyPressNotification notification, CancellationToken cancellationToken)
    {
        string invariantKey = notification.Key.ToUpperInvariant();

        if (invariantKey is ENTER_KEY)
        {
            await _mediator.Publish(new KeyEnterNotification(), cancellationToken);
        }

        if (invariantKey is "BACKSPACE")
        {
            await _mediator.Publish(new KeyBackspaceNotification(), cancellationToken);
        }

        if (invariantKey.Length is not 1 || invariantKey.Contains(DELIMITER))
        {
            return;
        }

        char invariantCharacter = invariantKey[0];

        if (ROW_1.Contains(invariantCharacter) || ROW_2.Contains(invariantCharacter) || ROW_3.Contains(invariantCharacter))
        {
            await _mediator.Publish(new KeySubmitNotification(invariantCharacter), cancellationToken);
        }
    }

    private void CreateKeys(string allKeysString, ObservableCollection<KeyViewModel> keyViewModels, bool widen = false)
    {
        string[] keys = allKeysString.Split(DELIMITER);

        int count = keys.Length + (widen ? 2 : 0);
        for (int i = 0; i < count; i++)
        {
            bool isWide = false;

            string text;
            if (widen && i is 0)
            {
                text = BACKSPACE_KEY;
                isWide = true;
            }
            else if (widen && i == keys.Length + 1)
            {
                text = ENTER_KEY;
                isWide = true;
            }
            else
            {
                int index = widen ? i - 1 : i;

                text = keys[index];
            }

            var keyViewModel = _viewModelFactory.Create<KeyViewModel>();

            char? invariantCharacter = text.Length is 1 ? char.ToUpperInvariant(text[0]) : null;

            keyViewModel.Create(text, isWide, invariantCharacter);

            keyViewModels.Add(keyViewModel);
        }
    }
}
