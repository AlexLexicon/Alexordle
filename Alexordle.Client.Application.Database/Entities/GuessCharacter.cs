using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class GuessCharacter : Character, IEntityTypeConfiguration<GuessCharacter>
{
    public required Guid GuessId { get; init; }
    public required int Row { get; init; }

    public void Configure(EntityTypeBuilder<GuessCharacter> builder)
    {
        builder
            .ToTable("GuessCharacters")
            .HasKey(gc => new
            {
                gc.PuzzleId,
                gc.Row,
                gc.Column,
            });
    }
}
