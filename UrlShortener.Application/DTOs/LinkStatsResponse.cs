namespace UrlShortener.Application.DTOs;

public record LinkStatsResponse(
    string ShortCode,
    string OriginalUrl,
    int TotalClicks,
    List<DailyClicks> ClicksByDay,
    List<DeviceStat> ByDevice,
    List<ReferrerStat> ByReferrer
);

public record DailyClicks(DateTime Date, int Count);
public record DeviceStat(string DeviceType, int Count);
public record ReferrerStat(string? Referrer, int Count);
