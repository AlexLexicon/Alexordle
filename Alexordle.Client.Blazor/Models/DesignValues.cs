namespace Alexordle.Client.Blazor.Models;
public class DesignValues
{
    public required int Width { get; init; }
    public required int MaxGuesses { get; init; }
    public required IReadOnlyList<string> Clues { get; init; }
    public required IReadOnlyList<string> Answers { get; init; }
    public required bool IsSpellChecking { get; init; }
}
