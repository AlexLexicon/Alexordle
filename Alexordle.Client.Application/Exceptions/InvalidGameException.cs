namespace Alexordle.Client.Application.Exceptions;
public class InvalidGameException(Exception? innerException) : Exception("The game was invalid.", innerException)
{
}
