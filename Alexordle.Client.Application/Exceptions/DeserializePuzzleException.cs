namespace Alexordle.Client.Application.Exceptions;
public class DeserializePuzzleException(string? serializedPuzzle, Exception? innerException) : Exception($"A failure occured while deserializing the puzzle '{serializedPuzzle ?? "null"}'.", innerException)
{
}
