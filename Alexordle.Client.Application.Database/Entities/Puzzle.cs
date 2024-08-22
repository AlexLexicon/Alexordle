using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Puzzle : IEntityTypeConfiguration<Puzzle>
{
    public required Guid Id { get; init; }
    public required int MaximumGuesses { get; set; }
    public required int Width { get; set; }
    public required bool IsSpellChecking { get; set; }

    public void Configure(EntityTypeBuilder<Puzzle> builder)
    {
        builder
            .ToTable("Puzzles")
            .HasKey(p => p.Id);
    }
}
