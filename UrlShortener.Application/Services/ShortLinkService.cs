using UrlShortener.Application.DTOs;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Services;

public class ShortLinkService
{
    private readonly IShortLinkRepository _repository;

    public ShortLinkService(IShortLinkRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShortLink> CreateShortLinkAsync(string originalUrl, Guid? ownerId, CancellationToken ct)
    {
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid URL format");

        var sequenceValue = await _repository.GetNextSequenceValueAsync(ct);
        var code = Base62Encoder.Encode(sequenceValue);

        var link = new ShortLink
        {
            Id = Guid.NewGuid(),
            OriginalUrl = originalUrl,
            ShortCode = code,
            CreatedAt = DateTime.UtcNow,
            OwnerId = ownerId,
            ClicksCount = 0
        };

        return await _repository.AddAsync(link, ct);
    }

    public async Task<ShortLink?> ResolveAsync(string code, CancellationToken ct)
    {
        var link = await _repository.GetByCodeAsync(code, ct);
        if (link == null || link.IsExpired()) return null;
        return link;
    }

    public async Task<IReadOnlyList<ShortLink>> GetMyLinksAsync(Guid ownerId, CancellationToken ct)
        => await _repository.GetByOwnerAsync(ownerId, ct);

    public async Task<LinkStatsResponse?> GetLinkStatsAsync(string code, Guid? requestingUserId, int month, int year, CancellationToken ct)
    {
        var link = await _repository.GetByCodeAsync(code, ct);
        if (link == null) return null;

        if (requestingUserId.HasValue && link.OwnerId != requestingUserId.Value)
            return null;

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);

        var clicksByDay = (await _repository.GetDailyStatsAsync(link.Id, from, to, ct))
            .Select(d => new DailyClicks(d.Date, d.Count))
            .ToList();

        var totalClicks = clicksByDay.Sum(d => d.Count);

        var clicks = await _repository.GetClicksByLinkAsync(link.Id, from, to, ct);

        var byDevice = clicks
            .GroupBy(c => c.DeviceType ?? "Unknown")
            .Select(g => new DeviceStat(g.Key, g.Count()))
            .OrderByDescending(d => d.Count)
            .ToList();

        var byReferrer = clicks
            .GroupBy(c => string.IsNullOrEmpty(c.Referrer) ? "Direct" : c.Referrer)
            .Select(g => new ReferrerStat(g.Key == "Direct" ? null : g.Key, g.Count()))
            .OrderByDescending(r => r.Count)
            .ToList();

        var byCountry = clicks
            .Where(c => !string.IsNullOrEmpty(c.Country))
            .GroupBy(c => c.Country!)
            .Select(g => new CountryStat(g.Key, g.Count()))
            .OrderByDescending(c => c.Count)
            .ToList();

        return new LinkStatsResponse(
            link.ShortCode,
            link.OriginalUrl,
            totalClicks,
            clicksByDay,
            byDevice,
            byReferrer,
            byCountry
        );
    }
}