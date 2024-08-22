namespace Alexordle.Client.Application.Services;
public interface IUrlService
{
    Task<string> GetUrlAsync();
    Task<string> GetPuzzleUrlAsync(string code);
}
