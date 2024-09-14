namespace Alexordle.Client.Application.Exceptions;
public class MalformedSerializedPuzzleException(string? serializedPuzzle) : Exception($"The serialized puzzle '{serializedPuzzle ?? "null"}' is malformed.")
{
}
