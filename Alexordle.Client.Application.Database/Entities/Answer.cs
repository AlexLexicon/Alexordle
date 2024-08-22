using Alexordle.Client.Application.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class Answer : AbstractWord, IEntityTypeConfiguration<Answer>
{
    public required string InvariantText { get; init; }

    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder
            .ToTable("Answers")
            .HasKey(a => a.WordId);
    }
}
