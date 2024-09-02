using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IHintService
{
    Task CalculateHintAsync(Character cell);
    Task PostCalculateHintsAsync<TCell>(IList<TCell> cells) where TCell : Character;
    Task<Hints> GetKeyboardHintAsync(Guid puzzleId, char invariantCharacter);
}
public class HintService : IHintService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;

    public HintService(IDbContextFactory<AlexordleDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task CalculateHintAsync(Character cell)
    {
        cell.Hint = await CalculateHintAsync(cell.PuzzleId, cell.Column, cell.InvariantCharacter, cell is HunchCharacter);
    }

    public async Task PostCalculateHintsAsync<TCell>(IList<TCell> cells) where TCell : Character
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        if (cells.Any(c => c.Hint is Hints.Elsewhere))
        {
            var invariantCharacterToPostHintCount = new Dictionary<char, PostHintCount>();
            for (int column = 0; column < cells.Count; column++)
            {
                Character cell = cells[column];

                if (cell.Hint is Hints.Elsewhere)
                {
                    char invariantCharcter = cell.InvariantCharacter;

                    bool exists = invariantCharacterToPostHintCount.ContainsKey(invariantCharcter);

                    PostHintCount postHintCount;
                    if (exists)
                    {
                        postHintCount = invariantCharacterToPostHintCount[invariantCharcter];
                    }
                    else
                    {
                        int inAnswerCount = await db.AnswerCharacters
                            .AsNoTracking()
                            .CountAsync(a => a.InvariantCharacter == cell.InvariantCharacter);

                        int inGuessCorrectCount = cells.Count(c => c.InvariantCharacter == cell.InvariantCharacter && c.Hint is Hints.Correct);

                        int remainingElsewheresCount = inAnswerCount - inGuessCorrectCount;

                        postHintCount = new PostHintCount
                        {
                            InvariantCharacter = invariantCharcter,
                            RemainingElsewheresCount = remainingElsewheresCount,
                            CurrentElsewhereCount = 0,
                        };
                    }

                    postHintCount.CurrentElsewhereCount++;

                    if (postHintCount.CurrentElsewhereCount > postHintCount.RemainingElsewheresCount)
                    {
                        cell.Hint = Hints.Incorrect;
                    }

                    if (exists)
                    {
                        invariantCharacterToPostHintCount[invariantCharcter] = postHintCount;
                    }
                    else
                    {
                        invariantCharacterToPostHintCount.Add(invariantCharcter, postHintCount);
                    }
                }
            }
        }
    }

    public async Task<Hints> GetKeyboardHintAsync(Guid puzzleId, char invariantCharacter)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        bool isAnswered = await db.AnswerCharacters
            .Where(ac => ac.PuzzleId == puzzleId && ac.InvariantCharacter == invariantCharacter)
            .Join(db.Answers, ac => ac.AnswerId, a => a.Id, (ac, a) => a)
            .Where(a => a.IsSolved)
            .AnyAsync();

        if (isAnswered)
        {
            return Hints.Incorrect;
        }

        ClueCharacter? clue = await db.ClueCharacters
            .AsNoTracking()
            .Where(c => c.PuzzleId == puzzleId && c.InvariantCharacter == invariantCharacter)
            .OrderByDescending(c => c.Hint)
            .FirstOrDefaultAsync();

        GuessCharacter? guess = await db.GuessCharacters
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
            Hints clueHint = clue!.Hint; //its odd that this thinks its possible for this to be null

            if (clueHint is > Hints.None)
            {
                return clueHint;
            }

            return Hints.Incorrect;
        }

        Hints guessHint = guess.Hint;

        if (guessHint is > Hints.None)
        {
            return guessHint;
        }

        return Hints.Incorrect;
    }

    private async Task<Hints> CalculateHintAsync(Guid puzzleId, int column, char invariantCharacter, bool isHunch)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        bool isCorrect = await db.AnswerCharacters
            .AsNoTracking()
            .AnyAsync(a => a.PuzzleId == puzzleId && a.Column == column && a.InvariantCharacter == invariantCharacter);

        if (isCorrect)
        {
            if (!isHunch)
            {
                return Hints.Correct;
            }

            bool isAlreadyKnownInColumn = await IsAlreadyKnownInColumn(db, column, invariantCharacter, Hints.Correct);
            if (isAlreadyKnownInColumn)
            {
                return Hints.Correct;
            }
        }

        bool isElsewhere = await db.AnswerCharacters
            .AsNoTracking()
            .AnyAsync(a => a.InvariantCharacter == invariantCharacter);

        if (isElsewhere)
        {
            if (!isHunch)
            {
                return Hints.Elsewhere;
            }

            bool isAlreadyKnownInColumn = await IsAlreadyKnownInColumn(db, column, invariantCharacter, Hints.None);
            if (isAlreadyKnownInColumn)
            {
                return Hints.Incorrect;
            }

            isAlreadyKnownInColumn = await IsAlreadyKnownInColumn(db, column, invariantCharacter, Hints.Elsewhere);
            if (isAlreadyKnownInColumn)
            {
                return Hints.Elsewhere;
            }

            return Hints.None;
        }

        if (isHunch)
        {
            bool isAlreadyKnown = await IsAlreadyKnown(db, invariantCharacter);

            return isAlreadyKnown ? Hints.Incorrect : Hints.None;
        }

        return Hints.Incorrect;
    }

    private async Task<bool> IsAlreadyKnownInColumn(AlexordleDbContext db, int column, char invariantCharacter, Hints hint)
    {
        bool alreadyUnKnown = await db.ClueCharacters
            .AsNoTracking()
            .AnyAsync(g => g.Column == column && g.InvariantCharacter == invariantCharacter && g.Hint == hint);

        if (alreadyUnKnown)
        {
            return true;
        }

        return await db.GuessCharacters
            .AsNoTracking()
            .AnyAsync(g => g.Column == column && g.InvariantCharacter == invariantCharacter && g.Hint == hint);
    }

    private async Task<bool> IsAlreadyKnown(AlexordleDbContext db, char invariantCharacter)
    {
        bool alreadyUnKnown = await db.ClueCharacters
            .AsNoTracking()
            .AnyAsync(g => g.InvariantCharacter == invariantCharacter);

        if (alreadyUnKnown)
        {
            return true;
        }

        return await db.GuessCharacters
            .AsNoTracking()
            .AnyAsync(g => g.InvariantCharacter == invariantCharacter);
    }
}
