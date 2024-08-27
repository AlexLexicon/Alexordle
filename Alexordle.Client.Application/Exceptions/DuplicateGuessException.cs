namespace Alexordle.Client.Application.Exceptions;
public class DuplicateGuessException(string? invariantText) : Exception($"The guess '{invariantText}' was already submitted.")
{
}
