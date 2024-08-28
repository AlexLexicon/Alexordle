namespace Alexordle.Client.Application.Services;
public interface IStorageService
{
    Task Store(string key, string value);
    Task<string?> Fetch(string key);
}
