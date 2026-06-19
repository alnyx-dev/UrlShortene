namespace UrlShortener.Domain.Entities;

public class ClickEvent
{
    public Guid Id { get; set; }
    public Guid ShortLinkId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Country { get; set; }
    public string? DeviceType { get; set; }
    public string? Referrer { get; set; }

    public ShortLink ShortLink { get; set; } = null!;
}