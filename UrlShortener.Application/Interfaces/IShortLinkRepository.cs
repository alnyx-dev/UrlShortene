using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IShortLinkRepository
{
    Task<ShortLink?> GetByCodeAsync(string code, CancellationToken ct);
    Task<ShortLink> AddAsync(ShortLink link, CancellationToken ct);
    Task<long> GetNextSequenceValueAsync(CancellationToken ct);
    Task AddClickAsync(ClickEvent click, CancellationToken ct);
    Task<IReadOnlyList<ShortLink>> GetByOwnerAsync(Guid ownerId, CancellationToken ct);
    Task<List<ClickEvent>> GetClicksByLinkAsync(Guid linkId, DateTime from, DateTime to, CancellationToken ct);
    Task UpdateClickCountryAsync(Guid clickId, string country, CancellationToken ct);
    Task<List<(DateTime Date, int Count)>> GetDailyStatsAsync(Guid linkId, DateTime from, DateTime to, CancellationToken ct);
}