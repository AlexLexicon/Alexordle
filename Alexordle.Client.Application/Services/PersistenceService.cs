using Alexordle.Client.Application.Database.Entities;
using Microsoft.Extensions.Logging;

namespace Alexordle.Client.Application.Services;
public interface IPersistenceService
{
    Task SaveAsync(Guid puzzleId);
    Task<Guid?> LoadAsync(string serializedPuzzle);
}
public class PersistenceService : IPersistenceService
{
    private readonly ILogger<PersistenceService> _logger;
    private readonly ISerializationService _serializationService;
    private readonly IStorageService _storageService;

    public PersistenceService(
        ILogger<PersistenceService> logger,
        ISerializationService serializationService,
        IStorageService storageService)
    {
        _logger = logger;
        _serializationService = serializationService;
        _storageService = storageService;
    }

    public async Task SaveAsync(Guid puzzleId)
    {
        var serializePuzzleTask = _serializationService.SerializePuzzleAsync(puzzleId);
        var serializePalleteTask = _serializationService.SerializePalleteAsync(puzzleId);

        string serializedPuzzle = await serializePuzzleTask;
        string serializedPallete = await serializePalleteTask;

        await _storageService.Store(serializedPuzzle, serializedPallete);
    }

    public async Task<Guid?> LoadAsync(string serializedPuzzle)
    {
        try
        {
            string? serializedPallete = await _storageService.Fetch(serializedPuzzle);

            if (serializedPallete is not null)
            {
                Puzzle puzzle = await _serializationService.DeserializeAndStartPuzzleAsync(serializedPuzzle);
                await _serializationService.DeserializePalleteAsync(puzzle.Id, serializedPallete);

                return puzzle.Id;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load the serialized puzzle '{serializedPuzzle}' from storage.", serializedPuzzle);
        }

        return null;
    }
}
