using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Guess : Cell, IEntityTypeConfiguration<Guess>
{
    public required int Row { get; init; }

    public void Configure(EntityTypeBuilder<Guess> builder)
    {
        builder
            .ToTable("Guesses")
            .HasKey(g => new
            {
                g.PuzzleId,
                g.Row,
                g.Column,
            });

        builder
            .Property(h => h.Hint)
            .HasConversion<string>();
    }
}
