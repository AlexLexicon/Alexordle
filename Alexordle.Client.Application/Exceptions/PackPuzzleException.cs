namespace Alexordle.Client.Application.Exceptions;
public class PackPuzzleException(Exception? innerException) : Exception("A failure occured while packing the puzzle.", innerException)
{
}
