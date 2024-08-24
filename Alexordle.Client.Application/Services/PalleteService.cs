using Alexordle.Client.Application.Database.Abstractions;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;

namespace Alexordle.Client.Application.Services;
public interface IPalleteService
{
    Task<Pallete> GeneratePalleteAsync(Guid puzzleId, State state, bool isDesigner);
}
public class PalleteService : IPalleteService
{
    private const char CHARACTER_EMPTY = ' ';

    private readonly IPuzzleService _puzzleService;
    private readonly IClueService _clueService;
    private readonly IGuessService _guessService;
    private readonly ILetterService _letterService;
    private readonly IHighlightServiceFactory _highlightServiceFactory;

    public PalleteService(
        IPuzzleService puzzleService,
        IClueService clueService,
        IGuessService guessService,
        ILetterService letterService,
        IHighlightServiceFactory highlightServiceFactory)
    {
        _puzzleService = puzzleService;
        _clueService = clueService;
        _guessService = guessService;
        _letterService = letterService;
        _highlightServiceFactory = highlightServiceFactory;
    }

    public async Task<Pallete> GeneratePalleteAsync(Guid puzzleId, State state, bool isDesigner)
    {
        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

        IHighlightService highlightService = await _highlightServiceFactory.CreateAndBuildHighlightServiceAsync(puzzleId);

        IReadOnlyList<Clue> clues = await _clueService.GetCluesAsync(puzzleId);

        var clueRows = new List<Row>();
        for (int index = 0; index < clues.Count; index++)
        {
            Row row = await GenerateRowAsync(highlightService, -1, puzzle.Width, clues[index], state, isDesignAnswers: false);

            clueRows.Add(row);
        }

        IReadOnlyList<Guess> guesses = await _guessService.GetGuessesAsync(puzzleId);
        int committedGuessesCount = guesses.Count(g => g.IsCommitted);

        int totalRows = Math.Max(puzzle.MaximumGuesses, committedGuessesCount) + (!state.IsVictory && !state.IsDefeat && state.IsBonus ? 1 : 0);

        var rows = new List<Row>();
        for (int index = 0; index < totalRows; index++)
        {
            Row row;
            if (index < guesses.Count)
            {
                Guess guess = guesses[index];

                row = await GenerateRowAsync(highlightService, index, puzzle.Width, guess, state, isDesigner);
            }
            else
            {
                row = await GenerateEmptyRowAsync(puzzle.Width);
            }

            rows.Add(row);
        }

        return new Pallete
        {
            Width = puzzle.Width,
            Clues = clueRows,
            Rows = rows,
        };
    }

    private async Task<Row> GenerateRowAsync(IHighlightService highlightService, int row, int width, AbstractWord word, State state, bool isDesignAnswers)
    {
        IReadOnlyList<Letter> letters = await _letterService.GetLettersAsync(word.WordId);

        Annotations annotation = Annotations.None;
        if (row >= state.MaximumGuesses && !isDesignAnswers)
        {
            annotation = Annotations.Bonus;
        }

        var cells = new List<Cell>();
        for (int position = 0; position < width; position++)
        {
            Cell cell;
            if (position < letters.Count)
            {
                Letter letter = letters[position];

                bool isCommitted = word is Guess guess && guess.IsCommitted || word is Clue;

                cell = await GenerateCellAsync(highlightService, letter.InvariantCharacter, position, isCommitted, isDesignAnswers);
            }
            else
            {
                cell = await GenerateEmptyCellAsync();
            }

            cells.Add(cell);
        }

        bool isFinished = await highlightService.PostProcessHighlightAsync(cells);
        if (!isDesignAnswers && isFinished && !state.CorrectGuessIds.Contains(word.WordId))
        {
            foreach (var cell in cells)
            {
                cell.IsFinished = false;
            }

            annotation = Annotations.Huh;
        }

        return new Row
        {
            Cells = cells,
            Annotation = annotation,
            IsGuessed = letters.Count == width,
        };
    }

    private async Task<Cell> GenerateCellAsync(IHighlightService highlightService, char? invariantCharacter, int position, bool isCommitted, bool isDesignAnswers)
    {
        string text = GetCellText(invariantCharacter);

        Highlights highlight = await highlightService.ProcessHighlightAsync(invariantCharacter, position, isCommitted, isDesignAnswers);

        return new Cell
        {
            InvariantCharacter = invariantCharacter ?? CHARACTER_EMPTY,
            Text = text,
            IsEmpty = false,
            IsObscured = false,
            IsFinished = false,
            Highlight = highlight,
        };
    }

    private async Task<Row> GenerateEmptyRowAsync(int width)
    {
        var cells = new List<Cell>();
        for (int count = 0; count < width; count++)
        {
            Cell cell = await GenerateEmptyCellAsync();

            cells.Add(cell);
        }

        return new Row
        {
            Cells = cells,
            Annotation = Annotations.None,
            IsGuessed = false,
        };
    }

    private Task<Cell> GenerateEmptyCellAsync()
    {
        string text = GetCellText(null);

        var cell = new Cell
        {
            InvariantCharacter = CHARACTER_EMPTY,
            Text = text,
            IsEmpty = true,
            IsObscured = false,
            IsFinished = false,
            Highlight = Highlights.None,
        };

        return Task.FromResult(cell);
    }

    private string GetCellText(char? character)
    {
        return character?.ToString() ?? string.Empty;
    }
}
