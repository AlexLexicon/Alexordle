using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;
using Microsoft.Extensions.Logging;

namespace Alexordle.Client.Application.Services;
public interface IPersistenceService
{
    Task SavePuzzleAsync(Guid puzzleId);
    Task SaveDesignAsync(Guid puzzleId);
    Task<Puzzle?> LoadPuzzleAsync(string serializedPuzzle);
    Task<Design?> LoadDesignAsync();
}
public class PersistenceService : IPersistenceService
{
    private const string KEY_DESIGN = "design";

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

    public async Task SavePuzzleAsync(Guid puzzleId)
    {
        var serializePuzzleTask = _serializationService.SerializePuzzleAsync(puzzleId);
        var serializePalleteTask = _serializationService.SerializePalleteAsync(puzzleId);

        string serializedPuzzle = await serializePuzzleTask;
        string serializedPallete = await serializePalleteTask;

        await _storageService.Store(serializedPuzzle, serializedPallete);
    }

    public async Task SaveDesignAsync(Guid puzzleId)
    {
        string serializedDesign = await _serializationService.SerializeDesignAsync(puzzleId);

        await _storageService.Store(KEY_DESIGN, serializedDesign);
    }

    public async Task<Puzzle?> LoadPuzzleAsync(string serializedPuzzle)
    {
        try
        {
            string? serializedPallete = await _storageService.Fetch(serializedPuzzle);

            if (serializedPallete is not null)
            {
                Puzzle puzzle = await _serializationService.DeserializeAndStartPuzzleAsync(serializedPuzzle);
                await _serializationService.DeserializePalleteAsync(puzzle.Id, serializedPallete);

                return puzzle;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load the serialized puzzle '{serializedPuzzle}' from storage.", serializedPuzzle);
        }

        return null;
    }

    public async Task<Design?> LoadDesignAsync()
    {
        string? serializedDesign = null;
        try
        {
            serializedDesign = await _storageService.Fetch(KEY_DESIGN);

            if (serializedDesign is not null)
            {
                return await _serializationService.DeserializeDesignAsync(serializedDesign);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load the serialized puzzle '{serializedDesign}' from storage.", serializedDesign);
        }

        return null;
    }
}
