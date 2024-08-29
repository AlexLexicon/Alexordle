using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Clue : IEntityTypeConfiguration<Clue>
{
    public required Guid Id { get; init; }
    public required Guid PuzzleId { get; init; }
    public required string InvariantText { get; init; }

    public void Configure(EntityTypeBuilder<Clue> builder)
    {
        builder
            .ToTable("Clues")
            .HasKey(c => c.Id);
    }
}
