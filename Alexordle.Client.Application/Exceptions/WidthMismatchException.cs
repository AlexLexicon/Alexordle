namespace Alexordle.Client.Application.Exceptions;
public class WidthMismatchException(string? text, int width, int puzzleWidth) : Exception($"The text '{text}' has a width of '{width}' but the puzzle has a width of '{puzzleWidth}'.")
{
}
