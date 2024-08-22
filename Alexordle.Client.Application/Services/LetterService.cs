using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Exceptions;
using Lexicom.DependencyInjection.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface ILetterService
{
    Task<string> GetStringAsync(Guid wordId);
    Task<int> GetLettersCountAsync(Guid wordId);
    Task<IReadOnlyList<Letter>> GetLettersAsync(Guid wordId);
    Task<char?> AppendLetterAsync(Guid wordId, char character);
    Task RemoveLetterAsync(Guid wordId);
    Task DeleteLettersAsync(Guid wordId);
    Task DeleteLetterAsync(Guid letterId);
}
public class LetterService : ILetterService
{
    public static string MakeInvariant(string word)
    {
        return word.ToUpperInvariant();
    }
    public static char MakeInvariant(char character)
    {
        return char.ToUpperInvariant(character);
    }

    private const string VALID_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly IGuidProvider _guidProvider;
    private readonly ITimestampService _timestampService;

    public LetterService(
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        IGuidProvider guidProvider,
        ITimestampService timestampService)
    {
        _dbContextFactory = dbContextFactory;
        _guidProvider = guidProvider;
        _timestampService = timestampService;
    }

    private HashSet<char>? _validInvariantCharacters;
    private HashSet<char> ValidInvariantCharacters
    {
        get
        {
            if (_validInvariantCharacters is null)
            {
                _validInvariantCharacters = [];
                foreach (char validCharacter in VALID_CHARACTERS)
                {
                    char validInvariantCharacters = MakeInvariant(validCharacter);

                    _validInvariantCharacters.Add(validInvariantCharacters);
                }
            }

            return _validInvariantCharacters;
        }
    }

    public async Task<string> GetStringAsync(Guid wordId)
    {
        IReadOnlyList<Letter> letters = await GetLettersAsync(wordId);

        string word = string.Empty;
        foreach (Letter letter in letters)
        {
            if (letter.InvariantCharacter is not null)
            {
                word += letter.InvariantCharacter;
            }
        }

        return word;
    }

    public async Task<int> GetLettersCountAsync(Guid wordId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Letters
            .AsNoTracking()
            .Where(l => l.WordId == wordId)
            .CountAsync();
    }

    public async Task<IReadOnlyList<Letter>> GetLettersAsync(Guid wordId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Letters
            .AsNoTracking()
            .Where(l => l.WordId == wordId)
            .OrderBy(l => l.CreatedTimestamp)
            .ToListAsync();
    }

    public async Task<char?> AppendLetterAsync(Guid wordId, char character)
    {
        char invariantWordCharacter = MakeInvariant(character);

        char? letterInvariantCharacter = null;
        if (ValidInvariantCharacters.Contains(invariantWordCharacter))
        {
            letterInvariantCharacter = invariantWordCharacter;
        }

        using var db = await _dbContextFactory.CreateDbContextAsync();

        long timestamp = await _timestampService.GetTimestampAsync();

        var letter = new Letter
        {
            Id = _guidProvider.NewGuid(),
            WordId = wordId,
            CreatedTimestamp = timestamp,
            InvariantCharacter = letterInvariantCharacter,
        };

        await db.AddAsync(letter);

        await db.SaveChangesAsync();

        return letterInvariantCharacter;
    }

    public async Task RemoveLetterAsync(Guid wordId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Letter? lastLetter = await db.Letters
            .AsNoTracking()
            .Where(l => l.WordId == wordId)
            .OrderBy(l => l.CreatedTimestamp)
            .LastOrDefaultAsync();

        if (lastLetter is not null)
        {
            await DeleteLetterAsync(lastLetter.Id);
        }
    }

    public async Task DeleteLettersAsync(Guid wordId)
    {
        IReadOnlyList<Letter> letters = await GetLettersAsync(wordId);

        foreach (Letter letter in letters)
        {
            await DeleteLetterAsync(letter.Id);
        }
    }

    public async Task DeleteLetterAsync(Guid letterId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Letter? letter = await db.Letters
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == letterId);

        if (letter is null)
        {
            throw new LetterDoesNotExistException(letterId);
        }

        db.Letters.Remove(letter);  

        await db.SaveChangesAsync();
    }
}
