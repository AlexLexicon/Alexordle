using Alexordle.Client.Application.Database.Entities;
using System.Text.Json;
using System.Text;
using System.Web;
using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Exceptions;

namespace Alexordle.Client.Application.Services;
public interface IGameService
{
    Task<string> EncodePuzzleAsync(Guid puzzleId);
    Task<Guid> LoadPuzzleFromCodeAsync(string code);
}
public class GameService : IGameService
{
    private readonly IPuzzleService _puzzleService;
    private readonly IClueService _clueService;
    private readonly IAnswerService _answerService;

    public GameService(
        IPuzzleService puzzleService,
        IClueService clueService,
        IAnswerService answerService)
    {
        _puzzleService = puzzleService;
        _clueService = clueService;
        _answerService = answerService;
    }

    public async Task<string> EncodePuzzleAsync(Guid puzzleId)
    {
        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

        IReadOnlyList<Clue> clues = await _clueService.GetCluesAsync(puzzle.Id);

        var clueStrings = new List<string>();
        foreach (Clue clue in clues)
        {
            clueStrings.Add(clue.InvariantText);    
        }

        IReadOnlyList<Answer> answers = await _answerService.GetAnswersAsync(puzzle.Id);

        var answerStrings = new List<string>();
        foreach (Answer answer in answers)
        {
            answerStrings.Add(answer.InvariantText);
        }

        var game = new Game
        {
            Clues = clueStrings,
            Answers = answerStrings,
            Width = puzzle.Width,
            MaximumGuesses = puzzle.MaximumGuesses,
            IsSpellChecking = puzzle.IsSpellChecking,
        };

        string json = JsonSerializer.Serialize(game);

        byte[] jsonBytes = Encoding.Default.GetBytes(json);

        string code = Convert.ToBase64String(jsonBytes);

        return HttpUtility.UrlEncode(code);
    }

    public async Task<Guid> LoadPuzzleFromCodeAsync(string code)
    {
        string decodedCode = HttpUtility.UrlDecode(code);

        byte[] jsonBytes = Convert.FromBase64String(decodedCode);

        string json = Encoding.Default.GetString(jsonBytes);

        Game? game = JsonSerializer.Deserialize<Game>(json);
        if (game is null)
        {
            throw new JsonNullDeserializationException();
        }

        Puzzle puzzle = await _puzzleService.CreatePuzzleAsync();

        await _puzzleService.SetWidthAsync(puzzle.Id, game.Width);
        await _puzzleService.SetMaximumGuessesAsync(puzzle.Id, game.MaximumGuesses);
        await _puzzleService.SetIsSpellCheckingAsync(puzzle.Id, game.IsSpellChecking);

        foreach (string clue in game.Clues)
        {
            await _clueService.CreateClueAsync(puzzle.Id, clue);
        }

        foreach (string answer in game.Answers)
        {
            await _answerService.CreateAnswerAsync(puzzle.Id, answer);
        }

        return puzzle.Id;
    }
}
