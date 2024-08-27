namespace Alexordle.Client.Application.Exceptions;
public class IncorrectSpellingException(string? invariantText) : Exception($"The invariant text '{invariantText ?? "null"}' was not in the word list.")
{
}
