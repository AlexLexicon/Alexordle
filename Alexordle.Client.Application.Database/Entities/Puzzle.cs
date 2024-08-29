using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Puzzle : IEntityTypeConfiguration<Puzzle>
{
    public const int VALIDATION_WIDTH_MINIMUM = 1;
    public const int VALIDATION_WIDTH_MAXIMUM = 10;
    public const int VALIDATION_MAXGUESSES_MINIMUM = 1;
    public const int VALIDATION_MAXGUESSES_MAXIMUM = 14;

    public required Guid Id { get; init; }
    public required int Width { get; init; }
    public required bool IsSpellChecking { get; init; }
    public required int TotalAnswers { get; init; }
    public required int CurrentAnswers { get; set; }
    public required int? MaxGuesses { get; init; }
    public required int CurrentGuesses { get; set; }
    public required bool IsFinished { get; set; }

    public void Configure(EntityTypeBuilder<Puzzle> builder)
    {
        builder
            .ToTable("Puzzles")
            .HasKey(p => p.Id);
    }
}
