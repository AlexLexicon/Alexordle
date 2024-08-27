namespace Alexordle.Client.Application.Exceptions;
public class IncompleteGuessException(int hunchWidth, int puzzleWidth) : Exception($"The hunch had a witdh of '{hunchWidth}' which is not equal to the puzzle width '{puzzleWidth}'.")
{
}
