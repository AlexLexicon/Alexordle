namespace Alexordle.Client.Application.Models;
public enum Highlights
{
    None, //no hightlighting
    Wrong, //white outline
    Elsewhere, //yellow outline
    Correct, //green outline
    Illegal, //red outline
    CommittedIncorrect, //gray filled
    CommittedElsewhere, //yellow filled
    CommittedCorrect, //green filled
    CommittedIllegal, //red filled
}
