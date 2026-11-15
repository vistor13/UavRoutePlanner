using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Options;

namespace UavRoutePlanner.BLL.Services;

public class LoginAttemptTracker(IOptions<AuthOptions> options, TimeProvider timeProvider) : ILoginAttemptTracker
{
    private readonly AuthOptions _options = options.Value;
    private readonly ConcurrentDictionary<string, Attempts> _attempts = new();

    public bool IsLocked(string userName)
    {
        if (!_attempts.TryGetValue(userName, out var attempts))
            return false;

        lock (attempts)
        {
            if (IsExpired(attempts))
            {
                _attempts.TryRemove(userName, out _);
                return false;
            }

            return attempts.Count >= _options.MaxFailedAttempts;
        }
    }

    public void RegisterFailure(string userName)
    {
        var now = timeProvider.GetUtcNow();
        var attempts = _attempts.GetOrAdd(userName, _ => new Attempts());

        lock (attempts)
        {
            if (IsExpired(attempts))
            {
                attempts.Count = 0;
            }

            attempts.Count++;
            attempts.LastFailureAt = now;
        }

        RemoveExpired();
    }

    public void Reset(string userName) => _attempts.TryRemove(userName, out _);

    private bool IsExpired(Attempts attempts) =>
        timeProvider.GetUtcNow() - attempts.LastFailureAt > TimeSpan.FromMinutes(_options.LockoutMinutes);

    private void RemoveExpired()
    {
        if (_attempts.Count < 1000) return;

        foreach (var pair in _attempts)
        {
            if (IsExpired(pair.Value)) _attempts.TryRemove(pair.Key, out _);
        }
    }

    private sealed class Attempts
    {
        public int Count { get; set; }

        public DateTimeOffset LastFailureAt { get; set; }
    }
}
