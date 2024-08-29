using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Exceptions;
using System.Text;
using System.Web;

namespace Alexordle.Client.Application.Services;
public interface ISerializationService
{
    Task<string> EncodeSerializedPuzzleForUrlAsync(string decodedBase64SerializedPuzzleString);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task<string> SerializePuzzleAsync(Guid puzzleId);
    Task<string> SerializePalleteAsync(Guid puzzleId);
    Task<Puzzle> DeserializeAndStartPuzzleAsync(string serializedPuzzle);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task DeserializePalleteAsync(Guid puzzleId, string serializedPallete);
}
public class SerializationService : ISerializationService
{
    private const char SERIALIZATION_DELIMITER_PRIMARY = ',';
    private const char SERIALIZATION_DELIMITER_SECONDARY = ';';
    private const int SERIALIZED_PUZZLE_PARTS_COUNT = 5 + 1;

    private readonly IPuzzleService _puzzleService;
    private readonly IAnswerService _answerService;
    private readonly IClueService _clueService;
    private readonly IGuessService _guessService;
    private readonly IHunchService _hunchService;

    public SerializationService(
        IPuzzleService puzzleService,
        IAnswerService answerService,
        IClueService clueService,
        IGuessService guessService,
        IHunchService hunchService)
    {
        _puzzleService = puzzleService;
        _answerService = answerService;
        _clueService = clueService;
        _guessService = guessService;
        _hunchService = hunchService;
    }

    public Task<string> EncodeSerializedPuzzleForUrlAsync(string decodedBase64SerializedPuzzleString)
    {
        string encodedBase64SerializedPuzzleString = HttpUtility.UrlEncode(decodedBase64SerializedPuzzleString);

        return Task.FromResult(encodedBase64SerializedPuzzleString);
    }

    public async Task<string> SerializePuzzleAsync(Guid puzzleId)
    {
        try
        {
            var getPuzzleTask = _puzzleService.GetPuzzleAsync(puzzleId);
            var getAnswerInvariantTextsTask = _answerService.GetAnswerInvariantTextsAsync(puzzleId);
            var getClueInvariantTextsTask = _clueService.GetClueInvariantTextsAsync(puzzleId);

            Puzzle puzzle = await getPuzzleTask;
            IReadOnlyList<string> answers = await getAnswerInvariantTextsTask;
            IReadOnlyList<string> clues = await getClueInvariantTextsTask;

            if (ContainsSpecialCharacters(clues) || ContainsSpecialCharacters(answers))
            {
                throw new Exception($"The special chracters '{SERIALIZATION_DELIMITER_PRIMARY}' and '{SERIALIZATION_DELIMITER_SECONDARY}' cannot be provided.");
            }

            string widthString = puzzle.Width.ToString();
            string maxGuessesString = (puzzle.MaxGuesses ?? -1).ToString();
            string isSpellCheckingString = (puzzle.IsSpellChecking ? 1 : 0).ToString();
            string cluesString = string.Join(SERIALIZATION_DELIMITER_SECONDARY, clues);
            string answersString = string.Join(SERIALIZATION_DELIMITER_SECONDARY, answers);

            if (ContainsSpecialCharacters(maxGuessesString, widthString, isSpellCheckingString))
            {
                throw new Exception($"The special chracters '{SERIALIZATION_DELIMITER_PRIMARY}' and '{SERIALIZATION_DELIMITER_SECONDARY}' cannot be provided.");
            }

            string decodedSerializedPuzzleString = string.Join(SERIALIZATION_DELIMITER_PRIMARY, maxGuessesString, widthString, isSpellCheckingString, cluesString, answersString);

            decodedSerializedPuzzleString += SERIALIZATION_DELIMITER_PRIMARY; //we append the delimiter at the end to better detect malformed codes.

            byte[] decodedSerializedPuzzleBytes = Encoding.Default.GetBytes(decodedSerializedPuzzleString);

            string decodedBase64SerializedPuzzleString = Convert.ToBase64String(decodedSerializedPuzzleBytes);

            return await EncodeSerializedPuzzleForUrlAsync(decodedBase64SerializedPuzzleString);
        }
        catch (Exception e)
        {
            throw new SerializePuzzleException(puzzleId, e);
        }
    }

    public async Task<string> SerializePalleteAsync(Guid puzzleId)
    {
        try
        {
            var getHunchesTask = _hunchService.GetHunchesAsync(puzzleId);

            string serializedPallete = string.Empty;

            int row = 0;
            bool complete = false;
            while (!complete)
            {
                IReadOnlyList<GuessCharacter> guesses = await _guessService.GetGuessesAsync(puzzleId, row);

                foreach (GuessCharacter guess in guesses)
                {
                    char invariantCharacter = guess.InvariantCharacter;
                    if (invariantCharacter is SERIALIZATION_DELIMITER_PRIMARY or SERIALIZATION_DELIMITER_SECONDARY)
                    {
                        throw new Exception($"Guess was the special chracter '{SERIALIZATION_DELIMITER_PRIMARY}' or '{SERIALIZATION_DELIMITER_SECONDARY}'.");
                    }

                    serializedPallete += $"{invariantCharacter}{SERIALIZATION_DELIMITER_SECONDARY}";
                }

                row++;
                complete = guesses.Count is <= 0;
            }

            serializedPallete = serializedPallete.TrimEnd(SERIALIZATION_DELIMITER_SECONDARY);
            serializedPallete += SERIALIZATION_DELIMITER_PRIMARY;

            IReadOnlyList<HunchCharacter> hunches = await getHunchesTask;
            foreach (HunchCharacter hunch in hunches)
            {
                char invariantCharacter = hunch.InvariantCharacter;
                if (invariantCharacter is SERIALIZATION_DELIMITER_PRIMARY or SERIALIZATION_DELIMITER_SECONDARY)
                {
                    throw new Exception($"Guess was the special chracter '{SERIALIZATION_DELIMITER_PRIMARY}' or '{SERIALIZATION_DELIMITER_SECONDARY}'.");
                }

                serializedPallete += $"{invariantCharacter}{SERIALIZATION_DELIMITER_SECONDARY}";
            }

            serializedPallete = serializedPallete.TrimEnd(SERIALIZATION_DELIMITER_SECONDARY);

            serializedPallete += $"{SERIALIZATION_DELIMITER_PRIMARY}";

            return serializedPallete;
        }
        catch (Exception e)
        {
            throw new SerializePalleteException(puzzleId, e);
        }
    }

    public async Task<Puzzle> DeserializeAndStartPuzzleAsync(string serializedPuzzle)
    {
        try
        {
            string packBase64 = HttpUtility.UrlDecode(serializedPuzzle);

            byte[] packBytes = Convert.FromBase64String(packBase64);

            string packString = Encoding.Default.GetString(packBytes);

            string[] parts = packString.Split(SERIALIZATION_DELIMITER_PRIMARY);

            if (parts.Length is > SERIALIZED_PUZZLE_PARTS_COUNT)
            {
                throw new Exception("too many parts.");
            }

            if (parts.Length is < SERIALIZED_PUZZLE_PARTS_COUNT)
            {
                throw new Exception("too few parts.");
            }

            string widthString = parts[0];
            string maxGuessesString = parts[1];
            string isSpellCheckingString = parts[2];
            string cluesString = parts[3];
            string answersString = parts[4];

            int maxGuesses = int.Parse(widthString);
            int width = int.Parse(maxGuessesString);
            bool isSpellChecking = int.Parse(isSpellCheckingString) is 1;
            string[] clues = cluesString.Split(SERIALIZATION_DELIMITER_SECONDARY);
            string[] answers = answersString.Split(SERIALIZATION_DELIMITER_SECONDARY);

            if (answers.Length is <= 0)
            {
                throw new Exception("no answers.");
            }

            int? nullableMaxGuesses = maxGuesses is > 0 ? maxGuesses : null;

            return await _puzzleService.StartPuzzleAsync(width, nullableMaxGuesses, isSpellChecking, clues, answers);
        }
        catch (Exception e)
        {
            throw new DeserializePuzzleException(serializedPuzzle, e);
        }
    }

    public async Task DeserializePalleteAsync(Guid puzzleId, string serializedPallete)
    {
        try
        {
            string[] parts = serializedPallete.Split(SERIALIZATION_DELIMITER_PRIMARY);

            if (parts.Length is not 3)
            {
                throw new Exception("There were not exactly two parts to the serialized pallete string.");
            }

            Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

            string guessesString = parts[0];
            string hunchesString = parts[1];

            string[] guesses = guessesString
                .Split(SERIALIZATION_DELIMITER_SECONDARY)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .ToArray();

            string[] hunches = hunchesString
                .Split(SERIALIZATION_DELIMITER_SECONDARY)
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToArray();

            if (guesses.Length is > 0)
            {
                if (guesses.Length % puzzle.Width is not 0)
                {
                    throw new Exception("The number of guesses does not match the mod width of the puzzle.");
                }

                int column = 0;
                foreach (string guess in guesses)
                {
                    if (guess.Length is <= 0)
                    {
                        throw new Exception("A guess was empty.");
                    }

                    if (guess.Length is > 1)
                    {
                        throw new Exception("A guess was more than one character.");
                    }

                    char character = guess[0];

                    await _hunchService.AppendCharacterToHunchAsync(puzzleId, character);

                    if (column >= puzzle.Width - 1)
                    {
                        await _hunchService.SubmitHunchAsync(puzzleId);

                        column = 0;
                    }
                    else
                    {
                        column++;
                    }
                }
            }

            if (hunches.Length is > 0)
            {
                foreach (string hunch in hunches)
                {
                    if (hunch.Length is <= 0)
                    {
                        throw new Exception("A hunch was empty.");
                    }

                    if (hunch.Length is > 1)
                    {
                        throw new Exception("A hunch was more than one character.");
                    }

                    char character = hunch[0];

                    await _hunchService.AppendCharacterToHunchAsync(puzzleId, character);
                }
            }
        }
        catch (Exception e)
        {
            throw new DeserializePalleteException(serializedPallete, e);
        }
    }

    private bool ContainsSpecialCharacters(params string[] parts) => ContainsSpecialCharacters(partsEnumerable: parts);
    private bool ContainsSpecialCharacters(IEnumerable<string> partsEnumerable)
    {
        foreach (string part in partsEnumerable)
        {
            if (part.Contains(SERIALIZATION_DELIMITER_PRIMARY) || part.Contains(SERIALIZATION_DELIMITER_SECONDARY))
            {
                return true;
            }
        }

        return false;
    }
}
