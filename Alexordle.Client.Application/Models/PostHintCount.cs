namespace Alexordle.Client.Application.Models;
public class PostHintCount
{
    public required char InvariantCharacter { get; init; }
    public required int RemainingElsewheresCount { get; init; }
    public required int CurrentElsewhereCount { get; set; }
}
