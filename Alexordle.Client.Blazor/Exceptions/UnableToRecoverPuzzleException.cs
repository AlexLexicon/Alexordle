namespace Alexordle.Client.Blazor.Exceptions;
public class UnableToRecoverPuzzleException(Guid puzzleId, string? serializedPuzzle) : Exception($"The puzzle with the orginal id '{puzzleId}' and the serialization '{serializedPuzzle ?? "null"}' could not be recovered.")
{
}
