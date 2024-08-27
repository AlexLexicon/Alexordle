namespace Alexordle.Client.Application.Models;
public class PuzzlePack
{
    public required int Width { get; init; }
    public required int? MaxGuesses { get; init; }
    public required bool IsSpellChecking { get; init; }
    public required IEnumerable<string> Clues { get; init; }
    public required IEnumerable<string> Answers { get; init; }
}
