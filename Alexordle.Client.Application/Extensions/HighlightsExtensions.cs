//using Alexordle.Client.Application.Models;

//namespace Alexordle.Client.Application.Extensions;
//public static class HighlightsExtensions
//{
//    public static bool IsCorrect(this HintTypes highlight)
//    {
//        return highlight switch
//        {
//            HintTypes.Correct => true,
//            HintTypes.CommittedCorrect => true,
//            _ => false,
//        };
//    }

//    public static bool IsElsewhere(this HintTypes highlight)
//    {
//        return highlight switch
//        {
//            HintTypes.Elsewhere => true,
//            HintTypes.CommittedElsewhere => true,
//            _ => false,
//        };
//    }

//    public static bool IsIllegal(this HintTypes highlight)
//    {
//        return highlight switch
//        {
//            HintTypes.Illegal => true,
//            HintTypes.CommittedIllegal => true,
//            _ => false,
//        };
//    }

//    public static HintTypes Better(this HintTypes highlight, HintTypes otherHighlight)
//    {
//        if (highlight is HintTypes.CommittedIllegal || otherHighlight is HintTypes.CommittedIllegal)
//        {
//            return HintTypes.CommittedIllegal;
//        }

//        if (highlight is HintTypes.CommittedCorrect || otherHighlight is HintTypes.CommittedCorrect)
//        {
//            return HintTypes.CommittedCorrect;
//        }

//        if (highlight is HintTypes.CommittedElsewhere || otherHighlight is HintTypes.CommittedElsewhere)
//        {
//            return HintTypes.CommittedElsewhere;
//        }

//        if (highlight is HintTypes.CommittedIncorrect || otherHighlight is HintTypes.CommittedIncorrect)
//        {
//            return HintTypes.CommittedIncorrect;
//        }

//        if (highlight is HintTypes.Illegal || otherHighlight is HintTypes.Illegal)
//        {
//            return HintTypes.Illegal;
//        }

//        if (highlight is HintTypes.Correct || otherHighlight is HintTypes.Correct)
//        {
//            return HintTypes.Correct;
//        }

//        if (highlight is HintTypes.Elsewhere || otherHighlight is HintTypes.Elsewhere)
//        {
//            return HintTypes.Elsewhere;
//        }

//        if (highlight is HintTypes.Wrong || otherHighlight is HintTypes.Wrong)
//        {
//            return HintTypes.Wrong;
//        }

//        return HintTypes.None;
//    }
//}
