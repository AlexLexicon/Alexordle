using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Exceptions;

namespace Alexordle.Client.Application.Services;
public interface IShareService
{
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task<string> GenerateShareAsync(Guid puzzleId);
}
public class ShareService : IShareService
{
    private readonly IPuzzleService _puzzleService;
    private readonly IUrlService _urlService;
    private readonly IClueService _clueService;
    private readonly IGuessService _guessService;
    private readonly ISerializationService _transmissionService;

    public ShareService(
        IPuzzleService puzzleService,
        IUrlService urlService,
        IClueService clueService,
        IGuessService guessService,
        ISerializationService transmissionService)
    {
        _puzzleService = puzzleService;
        _urlService = urlService;
        _clueService = clueService;
        _guessService = guessService;
        _transmissionService = transmissionService;
    }

    public async Task<string> GenerateShareAsync(Guid puzzleId)
    {
        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

        var serializePuzzleTask = _transmissionService.SerializePuzzleAsync(puzzleId);

        bool isDefeat = puzzle.IsFinished && puzzle.TotalAnswers - puzzle.CurrentAnswers > 0;

        string share = $"alexordle {(isDefeat ? "?" : puzzle.CurrentGuesses)}/{puzzle.MaxGuesses}{Environment.NewLine}";

        int row = 0;
        bool hasClues = false;
        bool complete = false;
        while (!complete)
        {
            IReadOnlyList<ClueCharacter> clues = await _clueService.GetCluesAsync(puzzleId, row);

            complete = clues.Count is <= 0;
            if (!complete)
            {
                foreach (ClueCharacter clue in clues)
                {
                    hasClues = true;

                    share += GetCell(clue.Hint);
                }

                share += Environment.NewLine;

                row++;
            }
        }

        if (hasClues)
        {
            for (int i = 0; i < puzzle.Width; i++)
            {
                share += GetCell(null);
            }

            share += Environment.NewLine;
        }

        row = 0;
        complete = false;
        while (!complete)
        {
            IReadOnlyList<GuessCharacter> guesses = await _guessService.GetGuessesAsync(puzzleId, row);

            complete = guesses.Count is <= 0;
            if (!complete)
            {

                foreach (GuessCharacter guess in guesses)
                {
                    share += GetCell(guess.Hint);
                }

                share += Environment.NewLine;

                row++;
            }
        }

        string serializedPuzzle = await serializePuzzleTask;

        share += await _urlService.CreatePuzzleUrlAsync(serializedPuzzle);

        return share;
    }

    private string GetCell(Hints? hint)
    {
        if (hint is null)
        {
            return "🔹";
        }
        //{
        //    return "🟥";
        //}
        else if (hint is Hints.Correct)
        {
            return "🟩";
        }
        else if (hint is Hints.Elsewhere)
        {
            return "🟨";
        }
        else
        {
            return "⬛";
        }
    }
}
