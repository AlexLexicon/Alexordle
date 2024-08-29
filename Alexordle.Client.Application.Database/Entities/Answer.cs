using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Answer : IEntityTypeConfiguration<Answer>
{
    public required Guid Id { get; init; }
    public required Guid PuzzleId { get; init; }
    public required string InvariantText { get; init; }
    public required bool IsSolved { get; set; }

    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder
            .ToTable("Answers")
            .HasKey(a => a.Id);
    }
}
