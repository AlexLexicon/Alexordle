using Lexicom.DependencyInjection.Primitives;

namespace Alexordle.Client.Application.Services;
public interface ITimestampService
{
    Task<long> GetTimestampAsync();
}
public class TimestampService : ITimestampService
{
    private readonly ITimeProvider _timeProvider;

    public TimestampService(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    private long PreviousTimestamp { get; set; }

    public async Task<long> GetTimestampAsync()
    {
        DateTimeOffset utcNow = _timeProvider.GetUtcNow();

        long timestamp = utcNow.Ticks;
        if (timestamp == PreviousTimestamp)
        {
            return await GetTimestampAsync();
        }

        PreviousTimestamp = timestamp;

        return timestamp;
    }
}
