using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Answer : IEntityTypeConfiguration<Answer>
{
    public const int VALIDATION_COUNT_MINIMUM = 1;
    public const int VALIDATION_COUNT_MAXIMUM = Puzzle.VALIDATION_MAXGUESSES_MAXIMUM;
    public const string VALIDATION_CHARACTERS_SUPPORTED = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public required Guid PuzzleId { get; init; }
    public required Guid AnswerId { get; init; }
    public required int Column { get; init; }
    public required char InvariantCharacter { get; init; }

    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder
            .ToTable("AnswerCharcters")
            .HasKey(ac => new
            {
                ac.PuzzleId,
                ac.AnswerId,
                ac.Column,
            });
    }
}
