namespace Alexordle.Client.Application.Exceptions;
public class HunchCharacterNotWhitelistedException(char character) : Exception($"The hunch character '{character}' is not in the whitelist.")
{
}
