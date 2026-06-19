using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Application.DTOs;
using UrlShortener.Application.Services;

namespace UrlShortener.Api.Pages;

[Authorize(AuthenticationSchemes = "Cookies")]
public class MyLinksModel : PageModel
{
    private readonly ShortLinkService _service;

    public MyLinksModel(ShortLinkService service)
    {
        _service = service;
    }

    public List<ShortLinkResponse> Links { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return;

        var userId = Guid.Parse(userIdClaim.Value);
        var links = await _service.GetMyLinksAsync(userId, ct);
        Links = links.Select(l => new ShortLinkResponse(
            l.ShortCode, l.OriginalUrl, l.CreatedAt, l.ExpiresAt, l.ClicksCount
        )).ToList();
    }
}
