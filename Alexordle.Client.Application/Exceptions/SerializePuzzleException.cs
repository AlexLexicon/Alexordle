namespace Alexordle.Client.Application.Exceptions;
public class SerializePuzzleException(Guid puzzleId, Exception? innerException) : Exception($"A failure occured while serializing the puzzle with the id '{puzzleId}'.", innerException)
{
}
