namespace Alexordle.Client.Application.Exceptions;
public class LetterDoesNotExistException(Guid letterId) : Exception($"The letter with the id '{letterId}' does not exist.")
{
}
