namespace Alexordle.Client.Application.Exceptions;
public class AnswerCharacterNotWhitelistedException(char character, string? answer) : Exception($"The character '{character}' in the answer '{answer ?? "null"}' is not in the whitelist.")
{
}
