﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alexordle.Client.Application.Database.Entities;
public class ClueCharacter : Character, IEntityTypeConfiguration<ClueCharacter>
{
    public const string VALIDATION_CHARACTERS_BLACKLIST = "⧕⧓⧗⧔";

    public required Guid ClueId { get; init; }
    public required int Row { get; init; }

    public void Configure(EntityTypeBuilder<ClueCharacter> builder)
    {
        builder
            .ToTable("ClueCharacters")
            .HasKey(cc => new
            {
                cc.PuzzleId,
                cc.Row,
                cc.Column,
            });
    }
}
