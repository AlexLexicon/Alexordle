using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Abstractions;
using Alexordle.Client.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IWordService
{
    Task<TWordEntity> GetWordAsync<TWordEntity>(Guid wordId) where TWordEntity : AbstractWord;
    Task CreateWordAsync<TWordEntity>(TWordEntity entity) where TWordEntity : AbstractWord;
    Task<string> WriteWordAsync(Guid wordId, string word);
    Task DeleteWordAsync<TWordEntity>(Guid wordId) where TWordEntity : AbstractWord;
}
public class WordService : IWordService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly ILetterService _letterService;

    public WordService(
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        ILetterService letterService)
    {
        _dbContextFactory = dbContextFactory;
        _letterService = letterService;
    }

    public async Task<TWordEntity> GetWordAsync<TWordEntity>(Guid wordId) where TWordEntity : AbstractWord
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await GetWordAsync<TWordEntity>(wordId, db);
    }

    public async Task CreateWordAsync<TWordEntity>(TWordEntity entity) where TWordEntity : AbstractWord
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        await db
            .Set<TWordEntity>()
            .AddAsync(entity);

        await db.SaveChangesAsync();
    }

    public async Task<string> WriteWordAsync(Guid wordId, string word)
    {
        string result = string.Empty;
        foreach (char character in word)
        {
            char? invariantCharacter = await _letterService.AppendLetterAsync(wordId, character);
            if (invariantCharacter is not null)
            {
                result += invariantCharacter;
            }
        }

        return result;
    }

    public async Task DeleteWordAsync<TWordEntity>(Guid wordId) where TWordEntity : AbstractWord
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        TWordEntity? entity = await GetWordAsync<TWordEntity>(wordId, db);

        db
            .Set<TWordEntity>()
            .Remove(entity);

        await _letterService.DeleteLettersAsync(wordId);

        await db.SaveChangesAsync();
    }

    private async Task<TWordEntity> GetWordAsync<TWordEntity>(Guid wordId, AlexordleDbContext db) where TWordEntity : AbstractWord
    {
        TWordEntity? entity = await db
            .Set<TWordEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(we => we.WordId == wordId);

        if (entity is null)
        {
            throw new WordDoesNotExistException(wordId, typeof(TWordEntity));
        }

        return entity;
    }
}
