using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class HunchCharacter : Character, IEntityTypeConfiguration<HunchCharacter>
{
    public void Configure(EntityTypeBuilder<HunchCharacter> builder)
    {
        builder
            .ToTable("HunchCharacters")
            .HasKey(hc => new
            {
                hc.PuzzleId,
                hc.Column,
            });

        builder
            .Property(hc => hc.Hint)
            .HasConversion<string>();
    }
}
