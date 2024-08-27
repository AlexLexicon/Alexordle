namespace Alexordle.Client.Application.Exceptions;
public class UnPackPuzzleException(Exception? innerException) : Exception("A failure occured while un packing the puzzle.", innerException)
{
}
