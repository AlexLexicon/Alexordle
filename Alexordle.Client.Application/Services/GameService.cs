//using Alexordle.Client.Application.Database.Entities;
//using Alexordle.Client.Application.Exceptions;
//using Alexordle.Client.Application.Models;
//using System.Text;
//using System.Web;

//namespace Alexordle.Client.Application.Services;
//public interface IGameService
//{
//    //Task<string> EncodePuzzleAsync(Guid puzzleId);
//    //Task<Guid> LoadPuzzleFromCodeAsync(string code);
//}
//public class GameService : IGameService
//{
//    //private readonly IPuzzleService _puzzleService;
//    //private readonly IClueService _clueService;
//    //private readonly IAnswerService _answerService;
//    //private readonly ISerializationService _serializationService;

//    //public GameService(
//    //    IPuzzleService puzzleService,
//    //    IClueService clueService,
//    //    IAnswerService answerService,
//    //    ISerializationService serializationService)
//    //{
//    //    _puzzleService = puzzleService;
//    //    _clueService = clueService;
//    //    _answerService = answerService;
//    //    _serializationService = serializationService;
//    //}

//    //public async Task<string> EncodePuzzleAsync(Guid puzzleId)
//    //{
//    //    var getPuzzleTask = _puzzleService.GetPuzzleAsync(puzzleId);
//    //    var GetCluesTask = _clueService.GetCluesAsync(puzzleId);

//    //    Puzzle puzzle = await getPuzzleTask;
//    //    IReadOnlyList < Clue > clues = await GetCluesTask;

//    //    var clueStrings = new List<string>();
//    //    foreach (Clue clue in clues)
//    //    {
//    //        clueStrings.Add(clue.InvariantText);
//    //    }

//    //    IReadOnlyList<Answer> answers = await _answerService.GetAnswersAsync(puzzle.Id);

//    //    var answerStrings = new List<string>();
//    //    foreach (Answer answer in answers)
//    //    {
//    //        answerStrings.Add(answer.InvariantText);
//    //    }

//    //    var game = new Game
//    //    {
//    //        Clues = clueStrings,
//    //        Answers = answerStrings,
//    //        Width = puzzle.Width,
//    //        MaximumGuesses = puzzle.MaxGuesses,
//    //        IsSpellChecking = puzzle.IsSpellChecking,
//    //    };

//    //    string gameString = await _serializationService.ConvertToStringAsync(game);

//    //    byte[] jsonBytes = Encoding.Default.GetBytes(gameString);

//    //    string code = Convert.ToBase64String(jsonBytes);

//    //    return HttpUtility.UrlEncode(code);
//    //}

//    //public async Task<Guid> LoadPuzzleFromCodeAsync(string code)
//    //{
//    //    try
//    //    {
//    //        string decodedCode = HttpUtility.UrlDecode(code);

//    //        byte[] jsonBytes = Convert.FromBase64String(decodedCode);

//    //        string gameString = Encoding.Default.GetString(jsonBytes);

//    //        var convertFromStringTask = _serializationService.ConvertFromStringAsync(gameString);
//    //        var createPuzzleTask = _puzzleService.CreatePuzzleAsync();

//    //        Game game = await convertFromStringTask;
//    //        Puzzle puzzle = await createPuzzleTask;

//    //        var setWidthTask = _puzzleService.SetWidthAsync(puzzle.Id, game.Width);
//    //        var setMaximumGuessesTask = _puzzleService.SetMaximumGuessesAsync(puzzle.Id, game.MaximumGuesses);
//    //        var setIsSpellCheckingTask = _puzzleService.SetIsSpellCheckingAsync(puzzle.Id, game.IsSpellChecking);

//    //        await setWidthTask;
//    //        await setMaximumGuessesTask;
//    //        await setIsSpellCheckingTask;

//    //        foreach (string clue in game.Clues)
//    //        {
//    //            await _clueService.CreateClueAsync(puzzle.Id, clue);
//    //        }

//    //        foreach (string answer in game.Answers)
//    //        {
//    //            await _answerService.CreateAnswerAsync(puzzle.Id, answer);
//    //        }

//    //        return puzzle.Id;
//    //    }
//    //    catch (Exception e)
//    //    {
//    //        throw new InvalidGameException(e);
//    //    }
//    //}
//}
