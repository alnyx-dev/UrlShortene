namespace UrlShortener.Application.DTOs;

public record ShortLinkResponse(
    string ShortCode,
    string OriginalUrl,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    int ClicksCount
);
