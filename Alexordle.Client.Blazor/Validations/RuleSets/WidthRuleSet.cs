using Alexordle.Client.Application.Database.Entities;
using FluentValidation;
using Lexicom.Validation;
using Lexicom.Validation.Amenities.Extensions;

namespace Alexordle.Client.Blazor.Validations.RuleSets;
public class WidthRuleSet : AbstractRuleSet<string?>
{
    public override void Use<T>(IRuleBuilderOptions<T, string?> ruleBuilder)
    {
        ruleBuilder
            .NotNull()
            .NotSimplyEmpty()
            .NotAnyWhiteSpace()
            .Digits()
            .GreaterThanOrEqualTo(Puzzle.VALIDATION_WIDTH_MINIMUM)
            .LessThanOrEqualTo(Puzzle.VALIDATION_WIDTH_MAXIMUM);
    }
}
