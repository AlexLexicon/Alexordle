namespace Alexordle.Client.Application.Models;
public class Cell
{
    public required char InvariantCharacter { get; init; }
    public required string Text { get; init; }
    public required bool IsEmpty { get; init; }
    public required bool IsObscured { get; init; }

    public required Highlights Highlight { get; set; }
    public required bool IsFinished { get; set; }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Text) ? " " : Text;
    }
}
