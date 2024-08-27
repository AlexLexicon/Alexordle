namespace Alexordle.Client.Application.Exceptions;
public abstract class CharacterNotSupportedException(string? thing, char character, string? text) : Exception($"The character '{character}'{(text is not null ? $" in '{text}'" : "")} is not a supported {thing} character.")
{
}
