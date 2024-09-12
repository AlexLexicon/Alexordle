namespace Alexordle.Client.Application.Models;
public class Design
{
    public required int Width { get; init; }
    public required int? MaxGuesses { get; init; }
    public required IReadOnlyList<string> Clues { get; init; }
    public required IReadOnlyList<string> Answers { get; init; }
    public required bool IsSpellChecking { get; init; }
}
