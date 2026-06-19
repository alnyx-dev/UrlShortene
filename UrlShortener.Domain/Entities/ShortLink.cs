namespace UrlShortener.Domain.Entities;

public class ShortLink
{
    public Guid Id { get; set; }
    public string OriginalUrl { get; set; } = null!;
    public string ShortCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? OwnerId { get; set; }
    public int ClicksCount { get; set; }

    public ICollection<ClickEvent> Clicks { get; set; } = new List<ClickEvent>();

    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
}