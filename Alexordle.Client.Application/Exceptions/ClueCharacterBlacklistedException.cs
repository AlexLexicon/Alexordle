namespace Alexordle.Client.Application.Exceptions;
public class ClueCharacterBlacklistedException(char character, string? clue) : Exception($"The character '{character}' in the clue '{clue ?? "null"}' is a black listed character.")
{
}
