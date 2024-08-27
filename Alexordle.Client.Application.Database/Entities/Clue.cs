using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Clue : Cell, IEntityTypeConfiguration<Clue>
{
    public const string VALIDATION_CHARACTERS_SUPPORTED = "ABCDEFGHIJKLMNOPQRSTUVWXYZ`1234567890-=~!@#$%^&*()_+[]\\{}|':\"./<>?";//BLACKLIST , ;

    public required int Row { get; init; }

    public void Configure(EntityTypeBuilder<Clue> builder)
    {
        builder
            .ToTable("Clues")
            .HasKey(c => new
            {
                c.PuzzleId,
                c.Row,
                c.Column,
            });

        builder
            .Property(h => h.Hint)
            .HasConversion<string>();
    }
}
