namespace Alexordle.Client.Application.Exceptions;
public class JsonNullDeserializationException() : Exception($"The deserialized json resulted in a null value.")
{
}
