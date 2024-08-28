namespace Alexordle.Client.Application.Exceptions;
public class SerializePalleteException(Guid puzzleId, Exception? innerException) : Exception($"A failure occured while serializing the pallete for the puzzle with the id '{puzzleId}'.", innerException)
{
}
