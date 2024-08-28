using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;
using System.Text;
using System.Web;

namespace Alexordle.Client.Application.Services;
public interface ISerializationService
{
    Task<Puzzle> DeserializeAndStartPuzzleAsync(string serializedPuzzle);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task<string> SerializePuzzleAsync(Guid puzzleId);
    Task DeserializePalleteAsync(Guid puzzleId, string serializedPallete);
    Task<string> SerializePalleteAsync(Guid puzzleId);
}
public class SerializationService : ISerializationService
{
    private readonly IPackService _packService;
    private readonly IPuzzleService _puzzleService;
    private readonly IAnswerService _answerService;
    private readonly IClueService _clueService;

    public SerializationService(
        IPackService packService,
        IPuzzleService puzzleService,
        IAnswerService answerService,
        IClueService clueService)
    {
        _packService = packService;
        _puzzleService = puzzleService;
        _answerService = answerService;
        _clueService = clueService;
    }

    public async Task<Puzzle> DeserializeAndStartPuzzleAsync(string serializedPuzzle)
    {
        string packBase64 = HttpUtility.UrlDecode(serializedPuzzle);

        byte[] packBytes = Convert.FromBase64String(packBase64);

        string packString = Encoding.Default.GetString(packBytes);

        PuzzlePack pack = await _packService.UnPackPuzzleAsync(packString);

        return await _puzzleService.StartPuzzleAsync(pack.Width, pack.MaxGuesses, pack.IsSpellChecking, pack.Clues, pack.Answers);
    }
 
    public async Task<string> SerializePuzzleAsync(Guid puzzleId)
    {
        var getPuzzleTask = _puzzleService.GetPuzzleAsync(puzzleId);
        var getAnswerInvariantTextsTask = _answerService.GetAnswerInvariantTextsAsync(puzzleId);
        var getClueInvariantTextsTask = _clueService.GetClueInvariantTextsAsync(puzzleId);

        Puzzle puzzle = await getPuzzleTask;
        IReadOnlyList<string> answers = await getAnswerInvariantTextsTask;
        IReadOnlyList<string> clues = await getClueInvariantTextsTask;

        string packString = await _packService.PackPuzzleAsync(new PuzzlePack
        {
            Width = puzzle.Width,
            MaxGuesses = puzzle.MaxGuesses,
            IsSpellChecking = puzzle.IsSpellChecking,
            Answers = answers,
            Clues = clues,
        });

        byte[] packBytes = Encoding.Default.GetBytes(packString);

        string packBase64 = Convert.ToBase64String(packBytes);

        return HttpUtility.UrlEncode(packBase64);
    }

    public Task DeserializePalleteAsync(Guid puzzleId, string serializedPallete)
    {
        throw new NotImplementedException();
    }

    public Task<string> SerializePalleteAsync(Guid puzzleId)
    {
        throw new NotImplementedException();
    }
}
