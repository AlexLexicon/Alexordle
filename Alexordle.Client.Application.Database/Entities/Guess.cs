using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Guess : IEntityTypeConfiguration<Guess>
{
    public required Guid Id { get; init; }
    public required Guid PuzzleId { get; init; }
    public required int Row { get; init; }
    public required string InvariantText { get; init; }
    public required bool IsAnswer { get; init; }
    public required bool IsBonus { get; init; }
    public required bool IsHuh { get; init; }

    public void Configure(EntityTypeBuilder<Guess> builder)
    {
        builder
            .ToTable("Guesses")
            .HasKey(g => g.Id);
    }
}
