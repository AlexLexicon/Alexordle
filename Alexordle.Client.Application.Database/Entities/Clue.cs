using Alexordle.Client.Application.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Clue : AbstractWord, IEntityTypeConfiguration<Clue>
{
    public required long CreatedTimestamp { get; init; }
    public required string InvariantText { get; init; }

    public void Configure(EntityTypeBuilder<Clue> builder)
    {
        builder
            .ToTable("Clues")
            .HasKey(a => a.WordId);
    }
}
