namespace Alexordle.Client.Application.Exceptions;
public class SerializationException(Exception? innerException) : Exception("Failed to parse the serialized string", innerException)
{
}
