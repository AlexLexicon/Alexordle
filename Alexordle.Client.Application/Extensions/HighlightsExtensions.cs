using Alexordle.Client.Application.Models;

namespace Alexordle.Client.Application.Extensions;
public static class HighlightsExtensions
{
    public static bool IsCorrect(this Highlights highlight)
    {
        return highlight switch
        {
            Highlights.Correct => true,
            Highlights.CommittedCorrect => true,
            _ => false,
        };
    }

    public static bool IsElsewhere(this Highlights highlight)
    {
        return highlight switch
        {
            Highlights.Elsewhere => true,
            Highlights.CommittedElsewhere => true,
            _ => false,
        };
    }

    public static bool IsIllegal(this Highlights highlight)
    {
        return highlight switch
        {
            Highlights.Illegal => true,
            Highlights.CommittedIllegal => true,
            _ => false,
        };
    }

    public static Highlights Better(this Highlights highlight, Highlights otherHighlight)
    {
        if (highlight is Highlights.CommittedIllegal || otherHighlight is Highlights.CommittedIllegal)
        {
            return Highlights.CommittedIllegal;
        }

        if (highlight is Highlights.CommittedCorrect || otherHighlight is Highlights.CommittedCorrect)
        {
            return Highlights.CommittedCorrect;
        }

        if (highlight is Highlights.CommittedElsewhere || otherHighlight is Highlights.CommittedElsewhere)
        {
            return Highlights.CommittedElsewhere;
        }

        if (highlight is Highlights.CommittedIncorrect || otherHighlight is Highlights.CommittedIncorrect)
        {
            return Highlights.CommittedIncorrect;
        }

        if (highlight is Highlights.Illegal || otherHighlight is Highlights.Illegal)
        {
            return Highlights.Illegal;
        }

        if (highlight is Highlights.Correct || otherHighlight is Highlights.Correct)
        {
            return Highlights.Correct;
        }

        if (highlight is Highlights.Elsewhere || otherHighlight is Highlights.Elsewhere)
        {
            return Highlights.Elsewhere;
        }

        if (highlight is Highlights.Wrong || otherHighlight is Highlights.Wrong)
        {
            return Highlights.Wrong;
        }

        return Highlights.None;
    }
}
