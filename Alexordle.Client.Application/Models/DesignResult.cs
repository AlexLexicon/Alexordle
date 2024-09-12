namespace Alexordle.Client.Application.Models;
public class DesignResult
{
    public required Guid PuzzleId { get; init; }
    public bool HasDuplicateAnswerCharacter { get; set; }
}
