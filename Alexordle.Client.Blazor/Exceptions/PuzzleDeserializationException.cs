namespace Alexordle.Client.Blazor.Exceptions;
public class PuzzleDeserializationException(string? serializedPuzzle) : Exception($"Failed to deserialize the puzzle '{serializedPuzzle}'.")
{
}
