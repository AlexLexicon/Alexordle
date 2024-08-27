namespace Alexordle.Client.Application.Exceptions;
public class ClueCharacterNotSupportedException(char character, string? clue) : CharacterNotSupportedException("clue", character, clue)
{
}
