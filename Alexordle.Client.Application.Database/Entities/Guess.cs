using Alexordle.Client.Application.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Guess : AbstractWord, IEntityTypeConfiguration<Guess>
{
    public required long CreatedTimestamp { get; init; }
    public required long CommittedTimestamp { get; init; }
    public required bool IsCommitted { get; set; }
    public required string? CommittedInvariantText { get; set; }

    public void Configure(EntityTypeBuilder<Guess> builder)
    {
        builder
            .ToTable("Guesses")
            .HasKey(a => a.WordId);
    }
}
