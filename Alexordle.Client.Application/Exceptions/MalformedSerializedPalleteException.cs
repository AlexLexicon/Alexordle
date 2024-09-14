namespace Alexordle.Client.Application.Exceptions;
public class MalformedSerializedPalleteException(string? serializedPallete) : Exception($"The serialized pallete '{serializedPallete ?? "null"}' is malformed.")
{
}
