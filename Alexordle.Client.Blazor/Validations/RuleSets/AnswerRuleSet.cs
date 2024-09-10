using FluentValidation;
using Lexicom.Validation;
using Lexicom.Validation.Amenities.Extensions;

namespace Alexordle.Client.Blazor.Validations.RuleSets;
public class AnswerRuleSet : AbstractRuleSet<string?>
{
    private readonly DesignerWidthValidation _designerWidthValidation;

    public AnswerRuleSet(DesignerWidthValidation designerWidthValidation)
    {
        _designerWidthValidation = designerWidthValidation;
    }

    public override void Use<T>(IRuleBuilderOptions<T, string?> ruleBuilder)
    {
        ruleBuilder
            .NotNull()
            .NotSimplyEmpty()
            .NotAnyWhiteSpace()
            .Letters()
            .Length(_ => _designerWidthValidation.CurrentWidth);
    }
}