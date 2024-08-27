namespace Alexordle.Client.Application.Exceptions;
public class DuplicateAnswerCharacterException(char character) : Exception($"Another answer already contains the character '{character}'.")
{
}
