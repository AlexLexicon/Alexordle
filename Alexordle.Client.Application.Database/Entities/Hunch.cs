using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Hunch : Cell, IEntityTypeConfiguration<Hunch>
{
    public void Configure(EntityTypeBuilder<Hunch> builder)
    {
        builder
            .ToTable("Hunch")
            .HasKey(h => new
            {
                h.PuzzleId,
                h.Column,
            });

        builder
            .Property(h => h.Hint)
            .HasConversion<string>();
    }
}
