namespace Alexordle.Client.Application.Services;
public interface IWordListsProvider
{
    Task<HashSet<string>> GetWordListForWidth1Async();
    Task<HashSet<string>> GetWordListForWidth2Async();
    Task<HashSet<string>> GetWordListForWidth3Async();
    Task<HashSet<string>> GetWordListForWidth4Async();
    Task<HashSet<string>> GetWordListForWidth5Async();
    Task<HashSet<string>> GetWordListForWidth6Async();
    Task<HashSet<string>> GetWordListForWidth7Async();
    Task<HashSet<string>> GetWordListForWidth8Async();
    Task<HashSet<string>> GetWordListForWidth9Async();
    Task<HashSet<string>> GetWordListForWidth10Async();
}
