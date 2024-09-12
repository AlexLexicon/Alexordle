namespace Alexordle.Client.Application.Exceptions;
public class DeserializeDesignException(string? serializedPuzzle, Exception? innerException) : Exception($"A failure occured while deserializing the design from the puzzle '{serializedPuzzle ?? "null"}'.", innerException)
{
}
