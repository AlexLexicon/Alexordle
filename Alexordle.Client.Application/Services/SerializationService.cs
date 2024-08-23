using Alexordle.Client.Application.Exceptions;
using Alexordle.Client.Application.Models;
using System.Text.Json;

namespace Alexordle.Client.Application.Services;
public interface ISerializationService
{
    Task<string> ConvertToStringAsync(Game game);
    Task<Game> ConvertFromStringAsync(string gameString);
}
public class JsonSerializationService : ISerializationService
{
    public Task<string> ConvertToStringAsync(Game game)
    {
        try
        {
            string json = JsonSerializer.Serialize(game);

            return Task.FromResult(json);
        }
        catch (Exception e)
        {
            throw new SerializationException(e);
        }
    }

    public Task<Game> ConvertFromStringAsync(string gameString)
    {
        try
        {
            Game? game = JsonSerializer.Deserialize<Game>(gameString);

            if (game is null)
            {
                throw new JsonNullDeserializationException();
            }

            return Task.FromResult(game);
        }
        catch (Exception e)
        {
            throw new SerializationException(e);
        }
    }
}
public class CustomSerializationService : ISerializationService
{
    public const char DELIMITER = ',';
    private const char DELIMITER_ARRAY = ';';

    public Task<string> ConvertToStringAsync(Game game)
    {
        try
        {
            string maximumGuesses = game.MaximumGuesses.ToString();
            string width = game.Width.ToString();
            string isSpellChecking = (game.IsSpellChecking ? 1 : 0).ToString();
            string clues = string.Join(DELIMITER_ARRAY, game.Clues);
            string answers = string.Join(DELIMITER_ARRAY, game.Answers);

            if (ContainsSpecialCharacters(maximumGuesses, width, isSpellChecking) || ContainsSpecialCharacters(game.Clues) || ContainsSpecialCharacters(game.Answers))
            {
                throw new Exception($"The special chracters '{DELIMITER}', '{DELIMITER_ARRAY}' are reserved and cannot be included in the game strings.");
            }

            string code = string.Join(DELIMITER, maximumGuesses, width, isSpellChecking, clues, answers);

            code += DELIMITER; //we append the delimiter at the end to better detect malformed codes.

            return Task.FromResult(code);
        }
        catch (Exception e)
        {
            throw new SerializationException(e);
        }
    }

    public Task<Game> ConvertFromStringAsync(string gameString)
    {
        try
        {
            string[] parts = gameString.Split(DELIMITER);

            if (parts.Length is not 6)
            {
                throw new Exception("too many or too few parts.");
            }

            int maximumGuesses = int.Parse(parts[0]);
            int width = int.Parse(parts[1]);
            bool isSpellChecking = int.Parse(parts[2]) is 1;
            string[] clues = parts[3].Split(DELIMITER_ARRAY);
            string[] answers = parts[4].Split(DELIMITER_ARRAY);

            if (answers.Length is <= 0)
            {
                throw new Exception("game string was malformed, no answers.");
            }

            var game = new Game
            {
                MaximumGuesses = maximumGuesses,
                Width = width,
                IsSpellChecking = isSpellChecking,
                Clues = clues.ToList(),
                Answers = answers.ToList(),
            };

            return Task.FromResult(game);
        }
        catch (Exception e)
        {
            throw new SerializationException(e);
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
