using System.Net;
using System.Text.Json;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.GeoIp;

public class GeoIpClient : IGeoIpClient
{
    private readonly HttpClient _httpClient;

    public GeoIpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(2);
    }

    public async Task<string?> GetCountryAsync(string? ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return null;

        if (ipAddress == "127.0.0.1" || ipAddress == "::1" || ipAddress.StartsWith("192.168.") || ipAddress.StartsWith("10."))
            return null;

        try
        {
            var response = await _httpClient.GetAsync($"http://ip-api.com/json/{ipAddress}?fields=country", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("country", out var countryProp))
            {
                var country = countryProp.GetString();
                return string.IsNullOrEmpty(country) ? null : country;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
