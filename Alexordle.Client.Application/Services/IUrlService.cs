namespace Alexordle.Client.Application.Services;
public interface IUrlService
{
    Task<string> GetUrlAsync();
    Task<string> CreatePuzzleUrlAsync(string serializedPuzzle);
}
