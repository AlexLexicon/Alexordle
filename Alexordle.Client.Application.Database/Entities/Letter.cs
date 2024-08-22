using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Letter : IEntityTypeConfiguration<Letter>
{
    public required Guid Id { get; init; }
    public required Guid WordId { get; init; }
    public required long CreatedTimestamp { get; init; }
    public required char? InvariantCharacter { get; init; }

    public void Configure(EntityTypeBuilder<Letter> builder)
    {
        builder
            .ToTable("Letters")
            .HasKey(l => l.Id);
    }
}
