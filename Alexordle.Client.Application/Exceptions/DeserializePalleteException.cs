namespace Alexordle.Client.Application.Exceptions;
public class DeserializePalleteException(string? serializedPallete, Exception? innerException) : Exception($"A failure occured while deserializing the pallete '{serializedPallete ?? "null"}'.", innerException)
{
}
