namespace Alexordle.Client.Application.Exceptions;
public class HunchCharacterNotSupportedException(char character) : CharacterNotSupportedException("hunch", character, text: null)
{
}
