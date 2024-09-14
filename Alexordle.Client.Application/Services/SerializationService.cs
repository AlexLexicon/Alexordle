using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Exceptions;
using Alexordle.Client.Application.Models;

namespace Alexordle.Client.Application.Services;
public interface ISerializationService
{
    /// <exception cref="SerializePuzzleException"></exception>
    Task<string> SerializePuzzleAsync(Guid puzzleId);
    /// <exception cref="SerializePalleteException"></exception>
    Task<string> SerializePalleteAsync(Guid puzzleId);
    /// <exception cref="SerializeDesignException"></exception>
    Task<string> SerializeDesignAsync(Guid puzzleId);
    /// <exception cref="DeserializePuzzleException"></exception>
    Task<Puzzle> DeserializeAndStartPuzzleAsync(string serializedPuzzle);
    /// <exception cref="DeserializePalleteException"></exception>
    Task DeserializePalleteAsync(Guid puzzleId, string serializedPallete);
    /// <exception cref="DeserializeDesignException"></exception>
    Task<Design> DeserializeDesignAsync(string base64SerializedDesign);
}
public class SerializationService : ISerializationService
{
    private const char SERIALIZATION_DELIMITER_START = '⧕';
    private const char SERIALIZATION_DELIMITER = '⧓';
    private const char SERIALIZATION_DELIMITER_END = '⧔';
    private const char SERIALIZATION_DELIMITER_NESTED = '⧗';
    private const int SERIALIZED_PUZZLE_PARTS_COUNT = 5;

    private readonly IPuzzleService _puzzleService;
    private readonly IAnswerService _answerService;
    private readonly IClueService _clueService;
    private readonly IGuessService _guessService;
    private readonly IHunchService _hunchService;
    private readonly IEncodingService _encodingService;

    public SerializationService(
        IPuzzleService puzzleService,
        IAnswerService answerService,
        IClueService clueService,
        IGuessService guessService,
        IHunchService hunchService,
        IEncodingService encodingService)
    {
        _puzzleService = puzzleService;
        _answerService = answerService;
        _clueService = clueService;
        _guessService = guessService;
        _hunchService = hunchService;
        _encodingService = encodingService;
    }

    public async Task<string> SerializePuzzleAsync(Guid puzzleId)
    {
        try
        {
            string base64SerializedPuzzle = await SerializePuzzleToBase64Async(puzzleId);

            return await _encodingService.UrlEncodeAsync(base64SerializedPuzzle);
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
            string serializedPallete = $"{SERIALIZATION_DELIMITER_START}";

            int row = 0;
            bool complete = false;
            while (!complete)
            {
                serializedPallete = await ConcatenateCharactersAsync<GuessCharacter>(serializedPallete, async () =>
                {
                    IReadOnlyList<GuessCharacter> guesses = await _guessService.GetGuessesAsync(puzzleId, row);

                    complete = guesses.Count is <= 0;

                    return guesses;
                });

                row++;
            }

            serializedPallete += SERIALIZATION_DELIMITER;

            serializedPallete = await ConcatenateCharactersAsync<HunchCharacter>(serializedPallete, async () =>
            {
                return await _hunchService.GetHunchesAsync(puzzleId);
            });

            return $"{serializedPallete}{SERIALIZATION_DELIMITER_END}";
        }
        catch (Exception e)
        {
            throw new SerializePalleteException(puzzleId, e);
        }
    }

    private async Task<string> ConcatenateCharactersAsync<TCharacter>(string serialization, Func<Task<IEnumerable<TCharacter>>> getCharactersDelegate) where TCharacter : Character
    {
        IEnumerable<TCharacter> characters = await getCharactersDelegate.Invoke();

        foreach (Character character in characters)
        {
            char invariantCharacter = character.InvariantCharacter;
            if (invariantCharacter is SERIALIZATION_DELIMITER_START or SERIALIZATION_DELIMITER or SERIALIZATION_DELIMITER_END)
            {
                throw new Exception($"A character was one of the blacklisted chracters '{SERIALIZATION_DELIMITER_START}', {SERIALIZATION_DELIMITER} or '{SERIALIZATION_DELIMITER_END}'.");
            }

            serialization += invariantCharacter;
        }

        return serialization;
    }

    public async Task<string> SerializeDesignAsync(Guid puzzleId)
    {
        try
        {
            return await SerializePuzzleToBase64Async(puzzleId);
        }
        catch (Exception e)
        {
            throw new SerializeDesignException(puzzleId, e);
        }
    }

    public async Task<Puzzle> DeserializeAndStartPuzzleAsync(string serializedPuzzle)
    {
        try
        {
            string base64SerializedPuzzleString = await _encodingService.UrlDecodeAsync(serializedPuzzle);

            Design design = await GetDesignFromSerializedBase64PuzzleAsync(base64SerializedPuzzleString);

            if (design.Answers.Count is <= 0)
            {
                throw new Exception("no answers.");
            }

            return await _puzzleService.StartPuzzleAsync(design.Width, design.MaxGuesses, design.IsSpellChecking, design.Clues, design.Answers);
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
            if (string.IsNullOrWhiteSpace(serializedPallete) || !serializedPallete.StartsWith(SERIALIZATION_DELIMITER_START) || !serializedPallete.EndsWith(SERIALIZATION_DELIMITER_END))
            {
                throw new MalformedSerializedPalleteException(serializedPallete);
            }

            //remove the start and end delimeter
            serializedPallete = serializedPallete[1..^1];

            string[] parts = serializedPallete.Split(SERIALIZATION_DELIMITER);

            if (parts.Length is not 2)
            {
                throw new Exception("There were not exactly two parts to the serialized pallete string.");
            }

            Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

            string guessesString = parts[0];
            string hunchesString = parts[1];

            var guesses = new List<char>();
            foreach (char character in guessesString)
            {
                guesses.Add(character);
            }

            var hunches = new List<char>();
            foreach (char character in hunchesString)
            {
                hunches.Add(character);
            }

            if (guesses.Count is > 0)
            {
                if (guesses.Count % puzzle.Width is not 0)
                {
                    throw new Exception("The number of guesses does not match the mod width of the puzzle.");
                }

                int column = 0;
                foreach (char guess in guesses)
                {
                    await _hunchService.AppendCharacterToHunchAsync(puzzleId, guess);

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

            if (hunches.Count is > 0)
            {
                foreach (char hunch in hunches)
                {
                    await _hunchService.AppendCharacterToHunchAsync(puzzleId, hunch);
                }
            }
        }
        catch (Exception e)
        {
            throw new DeserializePalleteException(serializedPallete, e);
        }
    }

    public async Task<Design> DeserializeDesignAsync(string base64SerializedDesign)
    {
        try
        {
            return await GetDesignFromSerializedBase64PuzzleAsync(base64SerializedDesign);
        }
        catch (Exception e)
        {
            throw new DeserializeDesignException(base64SerializedDesign, e);
        }
    }

    private async Task<string> SerializePuzzleToBase64Async(Guid puzzleId)
    {
        var getPuzzleTask = _puzzleService.GetPuzzleAsync(puzzleId);
        var getAnswerInvariantTextsTask = _answerService.GetAnswerInvariantTextsAsync(puzzleId);
        var getClueInvariantTextsTask = _clueService.GetClueInvariantTextsAsync(puzzleId);

        Puzzle puzzle = await getPuzzleTask;
        IReadOnlyList<string> answers = await getAnswerInvariantTextsTask;
        IReadOnlyList<string> clues = await getClueInvariantTextsTask;

        if (IsContainingReservedDelimiterCharacters(clues) || IsContainingReservedDelimiterCharacters(answers))
        {
            throw new Exception($"The reserved delimiter chracters '{SERIALIZATION_DELIMITER_START}', '{SERIALIZATION_DELIMITER}', '{SERIALIZATION_DELIMITER_NESTED}' and '{SERIALIZATION_DELIMITER_END}' cannot be used in a provided clue or answer.");
        }

        string widthString = puzzle.Width.ToString();
        string maxGuessesString = puzzle.MaxGuesses?.ToString() ?? string.Empty;
        string isSpellCheckingString = puzzle.IsSpellChecking ? "1" : string.Empty;
        string cluesString = string.Join(SERIALIZATION_DELIMITER_NESTED, clues);
        string answersString = string.Join(SERIALIZATION_DELIMITER_NESTED, answers);

        if (ContainsSpecialCharacters(maxGuessesString, widthString, isSpellCheckingString))
        {
            throw new Exception($"The reserved delimiter chracters '{SERIALIZATION_DELIMITER_START}', '{SERIALIZATION_DELIMITER}', '{SERIALIZATION_DELIMITER_NESTED}' and '{SERIALIZATION_DELIMITER_END}' was used in the design.");
        }

        string serializedPuzzle = string.Join(SERIALIZATION_DELIMITER, widthString, maxGuessesString, isSpellCheckingString, cluesString, answersString);

        serializedPuzzle = $"{SERIALIZATION_DELIMITER_START}{serializedPuzzle}{SERIALIZATION_DELIMITER_END}";

        byte[] serializedPuzzleBytes = await _encodingService.ByteEncodeAsync(serializedPuzzle);

        return await _encodingService.Base64EncodeAsync(serializedPuzzleBytes);
    }

    private async Task<Design> GetDesignFromSerializedBase64PuzzleAsync(string base64SerializedPuzzleString)
    {
        byte[] serializedPuzzleBytes = await _encodingService.Base64DecodeAsync(base64SerializedPuzzleString);

        string serializedPuzzle = await _encodingService.ByteDecodeAsync(serializedPuzzleBytes);

        if (string.IsNullOrWhiteSpace(serializedPuzzle) || !serializedPuzzle.StartsWith(SERIALIZATION_DELIMITER_START) || !serializedPuzzle.EndsWith(SERIALIZATION_DELIMITER_END))
        {
            throw new MalformedSerializedPuzzleException(serializedPuzzle);
        }

        //remove the start and end delimeter
        serializedPuzzle = serializedPuzzle[1..^1];

        string[] parts = serializedPuzzle.Split(SERIALIZATION_DELIMITER);

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

        int width = int.Parse(widthString);
        int? maxGuesses = string.IsNullOrWhiteSpace(maxGuessesString) ? null : int.Parse(maxGuessesString);
        bool isSpellChecking = isSpellCheckingString is "1";

        string[] clues = cluesString
            .Split(SERIALIZATION_DELIMITER_NESTED)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToArray();

        string[] answers = answersString.Split(SERIALIZATION_DELIMITER_NESTED);

        return new Design
        {
            Width = width,
            MaxGuesses = maxGuesses,
            IsSpellChecking = isSpellChecking,
            Clues = clues,
            Answers = answers,
        };
    }

    private bool ContainsSpecialCharacters(params string[] strings) => IsContainingReservedDelimiterCharacters(allStrings: strings);
    private bool IsContainingReservedDelimiterCharacters(IEnumerable<string> allStrings)
    {
        foreach (string str in allStrings)
        {
            foreach (char character in str)
            {
                if (character is SERIALIZATION_DELIMITER_START or SERIALIZATION_DELIMITER or SERIALIZATION_DELIMITER_END or SERIALIZATION_DELIMITER_NESTED)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
