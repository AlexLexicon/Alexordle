﻿namespace Alexordle.Client.Application.Models;
public class PuzzleDesign
{
    public required Guid PuzzleId { get; init; }
    public bool HasDuplicateAnswerCharacter { get; set; }
}
