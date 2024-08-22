namespace Alexordle.Client.Application.Services;
public interface IWordListProvider
{
    Task<HashSet<string>> LoadWords5Async();
}
