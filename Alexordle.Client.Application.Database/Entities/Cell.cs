using Alexordle.Client.Application.Database.Models;

namespace Alexordle.Client.Application.Database.Entities;
public abstract class Cell
{
    public required Guid PuzzleId { get; init; }
    public required int Column { get; init; }
    public required char InvariantCharacter { get; init; }
    public required Hints Hint { get; set; }
}
