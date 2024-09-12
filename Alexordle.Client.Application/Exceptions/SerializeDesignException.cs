namespace Alexordle.Client.Application.Exceptions;
public class SerializeDesignException(Guid puzzleId, Exception? innerException) : Exception($"A failure occured while serializing the design for the puzzle with the id '{puzzleId}'.", innerException)
{
}
