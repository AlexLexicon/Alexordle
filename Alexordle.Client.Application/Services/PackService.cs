using Alexordle.Client.Application.Exceptions;
using Alexordle.Client.Application.Models;
using System.Text.Json;

namespace Alexordle.Client.Application.Services;
public interface IPackService
{
    Task<string> PackPuzzleAsync(PuzzlePack puzzlePack);
    Task<PuzzlePack> UnPackPuzzleAsync(string puzzlePackString);
}
public class JsonPackService : IPackService
{
    public Task<string> PackPuzzleAsync(PuzzlePack puzzlePack)
    {
        try
        {
            string json = JsonSerializer.Serialize(puzzlePack);

            return Task.FromResult(json);
        }
        catch (Exception e)
        {
            throw new PackPuzzleException(e);
        }
    }

    public Task<PuzzlePack> UnPackPuzzleAsync(string gameString)
    {
        try
        {
            PuzzlePack? puzzlePack = JsonSerializer.Deserialize<PuzzlePack>(gameString);

            if (puzzlePack is null)
            {
                throw new JsonNullDeserializationException();
            }

            return Task.FromResult(puzzlePack);
        }
        catch (Exception e)
        {
            throw new UnPackPuzzleException(e);
        }
    }
}
public class CsvPackService : IPackService
{
    private const int PARTS_LENGTH = 5 + 1;//one more for malform detection
    private const char DELIMITER = ',';
    private const char DELIMITER_ARRAY = ';';

    public Task<string> PackPuzzleAsync(PuzzlePack pack)
    {
        try
        {
            if (ContainsSpecialCharacters(pack.Clues) || ContainsSpecialCharacters(pack.Answers))
            {
                throw new Exception($"The special chracters '{DELIMITER}' and '{DELIMITER_ARRAY}' cannot be provided.");
            }

            string widthString = pack.Width.ToString();
            string maxGuessesString = (pack.MaxGuesses ?? -1).ToString();
            string isSpellCheckingString = (pack.IsSpellChecking ? 1 : 0).ToString();
            string cluesString = string.Join(DELIMITER_ARRAY, pack.Clues);
            string answersString = string.Join(DELIMITER_ARRAY, pack.Answers);

            if (ContainsSpecialCharacters(maxGuessesString, widthString, isSpellCheckingString))
            {
                throw new Exception($"The special chracters '{DELIMITER}' and '{DELIMITER_ARRAY}' cannot be provided.");
            }

            string packString = string.Join(DELIMITER, maxGuessesString, widthString, isSpellCheckingString, cluesString, answersString);

            packString += DELIMITER; //we append the delimiter at the end to better detect malformed codes.

            return Task.FromResult(packString);
        }
        catch (Exception e)
        {
            throw new PackPuzzleException(e);
        }
    }

    public Task<PuzzlePack> UnPackPuzzleAsync(string packString)
    {
        try
        {
            string[] parts = packString.Split(DELIMITER);

            if (parts.Length is > PARTS_LENGTH)
            {
                throw new Exception("too many parts.");
            }

            if (parts.Length is < PARTS_LENGTH)
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
            string[] clues = cluesString.Split(DELIMITER_ARRAY);
            string[] answers = answersString.Split(DELIMITER_ARRAY);

            if (answers.Length is <= 0)
            {
                throw new Exception("no answers.");
            }

            var puzzlePack = new PuzzlePack
            {
                Width = width,
                MaxGuesses = maxGuesses is > 0 ? maxGuesses : null,
                IsSpellChecking = isSpellChecking,
                Clues = clues,
                Answers = answers,
            };

            return Task.FromResult(puzzlePack);
        }
        catch (Exception e)
        {
            throw new UnPackPuzzleException(e);
        }
    }

    private bool ContainsSpecialCharacters(params string[] parts) => ContainsSpecialCharacters(partsEnumerable: parts);
    private bool ContainsSpecialCharacters(IEnumerable<string> partsEnumerable)
    {
        foreach (string part in partsEnumerable)
        {
            if (part.Contains(DELIMITER) || part.Contains(DELIMITER_ARRAY))
            {
                return true;
            }
        }

        return false;
    }
}
