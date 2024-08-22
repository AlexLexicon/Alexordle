namespace Alexordle.Client.Application.Database.Abstractions;
public class AbstractWord
{
    public required Guid WordId { get; init; }
    public required Guid PuzzleId { get; init; }
}
