using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Hunch : IEntityTypeConfiguration<Hunch>
{
    public required Guid PuzzleId { get; init; }
    public required bool IsBonus { get; set; }
    public required bool IsInfinite { get; init; }

    public void Configure(EntityTypeBuilder<Hunch> builder)
    {
        builder
            .ToTable("Hunches")
            .HasKey(h => h.PuzzleId);
    }
}