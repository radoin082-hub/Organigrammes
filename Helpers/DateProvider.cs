namespace Compta.Ledger.Core.orgTestapp.Helpers;

public interface IDateProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }

    DateOnly TodayUtc { get; }
    DateOnly TodayLocal { get; }

    TimeOnly TimeUtc { get; }
    TimeOnly TimeLocal { get; }

    long UnixSeconds { get; }
    long UnixMilliseconds { get; }

    DateTime FromUnixSeconds(long seconds);
    DateTime FromUnixMilliseconds(long milliseconds);
}

public sealed class DateProvider : IDateProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;

    public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly TodayLocal => DateOnly.FromDateTime(DateTime.Now);

    public TimeOnly TimeUtc => TimeOnly.FromDateTime(DateTime.UtcNow);
    public TimeOnly TimeLocal => TimeOnly.FromDateTime(DateTime.Now);

    public long UnixSeconds =>
        DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public long UnixMilliseconds =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public DateTime FromUnixSeconds(long seconds) =>
        DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

    public DateTime FromUnixMilliseconds(long milliseconds) =>
        DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
}

