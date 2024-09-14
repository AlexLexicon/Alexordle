namespace Alexordle.Client.Application.Exceptions;
public class ClueNotEncodableException(string? clue) : Exception($"The clue '{clue ?? "null"}' is not encodable.")
{
}
