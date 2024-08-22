namespace Alexordle.Client.Application.Models;
public class State
{
    public required int TotalGuesses { get; init; }
    public required int MaximumGuesses { get; init; }
    public required int RemainingAnswers { get; init; }
    public required int RemainingGuesses { get; init; }
    public required bool IsBonus { get; init; }
    public required bool IsDefeat { get; init; }
    public required bool IsVictory { get; init; }
    public required IReadOnlySet<Guid> CorrectGuessIds { get; init; }
}
