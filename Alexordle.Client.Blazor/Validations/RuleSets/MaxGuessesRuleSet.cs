using Alexordle.Client.Application.Database.Entities;
using FluentValidation;
using Lexicom.Validation;
using Lexicom.Validation.Amenities.Extensions;

namespace Alexordle.Client.Blazor.Validations.RuleSets;
public class MaxGuessesRuleSet : AbstractRuleSet<string?>
{
    private readonly DesignerAnswersCountValidation _designerAnswersCountValidation;

    public MaxGuessesRuleSet(DesignerAnswersCountValidation designerAnswersCountValidation)
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
            .GreaterThanOrEqualTo(() => Math.Max(_designerAnswersCountValidation.CurrentAnswersCount, Puzzle.VALIDATION_MAXGUESSES_MINIMUM))
            .LessThanOrEqualTo(() => _designerAnswersCountValidation.CurrentAnswersCount + Puzzle.VALIDATION_MAXGUESSES_MAXIMUM);
    }
}