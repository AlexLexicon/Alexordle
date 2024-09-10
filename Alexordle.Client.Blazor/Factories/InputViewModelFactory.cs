using Alexordle.Client.Blazor.ViewModels.Designer;
using Lexicom.Mvvm;
using Lexicom.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Alexordle.Client.Blazor.Factories;
public interface IInputViewModelFactory
{
    InputViewModel CreateInputViewModel<TRuleSet>(string label) where TRuleSet : IRuleSet<string?>;
    ListInputViewModel CreateListInputViewModel<TRuleSet>() where TRuleSet : IRuleSet<string?>;
}
public class InputViewModelFactory : IInputViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IViewModelFactory _viewModelFactory;

    public InputViewModelFactory(
        IServiceProvider serviceProvider,
        IViewModelFactory viewModelFactory)
    {
        _serviceProvider = serviceProvider;
        _viewModelFactory = viewModelFactory;
    }

    public InputViewModel CreateInputViewModel<TRuleSet>(string label) where TRuleSet : IRuleSet<string?>
    {
        var validator = _serviceProvider.GetRequiredService<IRuleSetValidator<TRuleSet, string?>>();

        var inputViewModel = _viewModelFactory.Create<InputViewModel, IRuleSetValidator<TRuleSet, string?>>(validator);

        inputViewModel.Label = label;

        return inputViewModel;
    }

    public ListInputViewModel CreateListInputViewModel<TRuleSet>() where TRuleSet : IRuleSet<string?>
    {
        var validator = _serviceProvider.GetRequiredService<IRuleSetValidator<TRuleSet, string?>>();

        return _viewModelFactory.Create<ListInputViewModel, IRuleSetValidator<TRuleSet, string?>>(validator);
    }
}
