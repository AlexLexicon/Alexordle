namespace Alexordle.Client.Application.Exceptions;
public class GuessDoesNotExistException(Guid guessId) : Exception($"The guess with the id '{guessId}' does not exist.")
{
}
