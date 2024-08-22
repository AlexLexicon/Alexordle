namespace Alexordle.Client.Application.Exceptions;
public class PuzzleDoesNotExistException(Guid puzzleId) : Exception($"The puzzle with the id '{puzzleId}' does not exist.")
{
}
