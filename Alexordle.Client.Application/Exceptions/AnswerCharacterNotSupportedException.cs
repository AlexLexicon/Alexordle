namespace Alexordle.Client.Application.Exceptions;
public class AnswerCharacterNotSupportedException(char character, string? answer) : CharacterNotSupportedException("answer", character, answer)
{
}
