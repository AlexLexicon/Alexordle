using FluentValidation;
using Lexicom.Validation;
using Lexicom.Validation.Amenities.Extensions;

namespace Alexordle.Client.Blazor.Validations.RuleSets;
public class MaximumGuessesRuleSet : AbstractRuleSet<string?>
{
    private readonly DesignerAnswersCountValidation _designerAnswersCountValidation;

    public MaximumGuessesRuleSet(DesignerAnswersCountValidation designerAnswersCountValidation)
    {
        _designerAnswersCountValidation = designerAnswersCountValidation;
    }

    public override void Use<T>(IRuleBuilderOptions<T, string?> ruleBuilder)
    {
        ruleBuilder
            .NotNull()
            .NotSimplyEmpty()
            .NotAnyWhiteSpace()
            .Digits()
            .GreaterThanOrEqualTo(() => _designerAnswersCountValidation.CurrentAnswersCount)
            .LessThanOrEqualTo(() => _designerAnswersCountValidation.CurrentAnswersCount + 20);
    }
}