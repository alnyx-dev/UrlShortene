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
}