namespace Alexordle.Client.Application.Exceptions;
public class HunchDoesNotExistException(Guid puzzleId) : Exception($"The hunch for the puzzle id '{puzzleId}' does not exist.")
{
}
