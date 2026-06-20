namespace UrlShortener.Application.Interfaces;

public interface IGeoIpClient
{
    Task<string?> GetCountryAsync(string? ipAddress, CancellationToken ct);
}
