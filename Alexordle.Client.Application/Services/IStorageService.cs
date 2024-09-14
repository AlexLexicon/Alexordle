namespace Alexordle.Client.Application.Services;
public interface IStorageService
{
    Task ClearAsync();
    Task StoreAsync(string key, string value);
    Task<string?> FetchAsync(string key);
}
