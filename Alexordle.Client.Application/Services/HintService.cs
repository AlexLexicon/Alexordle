using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IHintService
{
    Task CalculateHintAsync(Cell cell);
    Task PostCalculateHintsAsync<TCell>(IList<TCell> cells) where TCell : Cell;
    Task<Hints> GetHighestHintAsync(Guid puzzleId, char invariantCharacter);
}
public class HintService : IHintService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;

    public HintService(IDbContextFactory<AlexordleDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task CalculateHintAsync(Cell cell)
    {
        cell.Hint = await CalculateHintAsync(cell.PuzzleId, cell.Column, cell.InvariantCharacter, cell is Hunch);
    }

    public async Task PostCalculateHintsAsync<TCell>(IList<TCell> cells) where TCell : Cell
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        for (int count = 0; count < cells.Count; count++)
        {
            Cell cell = cells[count];

            if (cell.Hint is Hints.Elsewhere)
            {
                int answerInvariantCharacterCount = await db.Answers
                    .AsNoTracking()
                    .CountAsync(a => a.InvariantCharacter == cell.InvariantCharacter);

                if (answerInvariantCharacterCount is > 1)
                {
                    int correctInvariantCharacterCount = cells.Count(g => g.InvariantCharacter == cell.InvariantCharacter);

                    if (count > correctInvariantCharacterCount)
                    {
                        cell.Hint = Hints.None;
                    }
                }
            }
        }
    }

    public async Task<Hints> GetHighestHintAsync(Guid puzzleId, char invariantCharacter)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Clue? clue = await db.Clues
            .AsNoTracking()
            .Where(c => c.PuzzleId == puzzleId && c.InvariantCharacter == invariantCharacter)
            .OrderByDescending(c => c.Hint)
            .FirstOrDefaultAsync();

        Guess? guess = await db.Guesses
            .AsNoTracking()
            .Where(g => g.PuzzleId == puzzleId && g.InvariantCharacter == invariantCharacter)
            .OrderByDescending(g => g.Hint)
            .FirstOrDefaultAsync();

        if (guess is null && clue is null)
        {
            return Hints.None;
        }

        if (guess is null)
        {
            //its odd that this thinks its possible for this to be null
            return clue!.Hint;
        }

        if (clue is null)
        {
            return guess.Hint;
        }

        if (guess.Hint > clue.Hint)
        {
            return guess.Hint;
        }
        else
        {
            return clue.Hint;
        }
    }

    private async Task<Hints> CalculateHintAsync(Guid puzzleId, int column, char invariantCharacter, bool isHunch)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        bool isCorrect = await db.Answers
            .AsNoTracking()
            .AnyAsync(a => a.PuzzleId == puzzleId && a.Column == column && a.InvariantCharacter == invariantCharacter);

        if (isCorrect)
        {
            if (!isHunch)
            {
                return Hints.Correct;
            }

            bool isAlreadyKnown = await IsAlreadyKnown(db, column, invariantCharacter, Hints.Correct);
            if (isAlreadyKnown)
            {
                return Hints.Correct;
            }
        }

        bool isElsewhere = await db.Answers
            .AsNoTracking()
            .AnyAsync(a => a.InvariantCharacter == invariantCharacter);

        if (isElsewhere)
        {
            if (!isHunch)
            {
                return Hints.Elsewhere;
            }

            bool isAlreadyKnown = await IsAlreadyKnown(db, column, invariantCharacter, Hints.None);
            if (isAlreadyKnown)
            {
                return Hints.Wrong;
            }

            isAlreadyKnown = await IsAlreadyKnown(db, column, invariantCharacter, Hints.Elsewhere);
            if (isAlreadyKnown)
            {
                return Hints.Elsewhere;
            }
        }

        if (isHunch)
        {
            bool isAlreadyKnown = await IsAlreadyKnown(db, column, invariantCharacter, Hints.None);
            if (isAlreadyKnown)
            {
                return Hints.Wrong;
            }
        }

        return Hints.None;
    }

    private async Task<bool> IsAlreadyKnown(AlexordleDbContext db, int column, char invariantCharacter, Hints hint)
    {
        bool alreadyUnKnown = await db.Clues
            .AsNoTracking()
            .AnyAsync(g => g.Column == column && g.InvariantCharacter == invariantCharacter && g.Hint == hint);

        if (alreadyUnKnown)
        {
            return true;
        }

        return await db.Guesses
            .AsNoTracking()
            .AnyAsync(g => g.Column == column && g.InvariantCharacter == invariantCharacter && g.Hint == hint);
    }

    //private async Task<Hints> CalculateGuessHintAsync(AlexordleDbContext db, Guid puzzleId, char invariantCharacter, int column)
    //{
    //    bool isCorrect = await db.Answers
    //        .AsNoTracking()
    //        .AnyAsync(a => a.PuzzleId == puzzleId && a.Column == column && a.InvariantCharacter == invariantCharacter);

    //    if (isCorrect)
    //    {
    //        return Hints.Correct;
    //    }

    //    bool isElsewhere = await db.Answers
    //        .AsNoTracking()
    //        .AnyAsync(a => a.InvariantCharacter == invariantCharacter);

    //    if (isElsewhere)
    //    {
    //        return Hints.Elsewhere;
    //    }

    //    return Hints.None;
    //}
}
