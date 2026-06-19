using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Application.Services;

namespace UrlShortener.Api.Pages;

public class IndexModel : PageModel
{
    private readonly ShortLinkService _service;

    public IndexModel(ShortLinkService service)
    {
        _service = service;
    }

    [BindProperty]
    public string OriginalUrl { get; set; } = string.Empty;

    public string? GeneratedShortUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(OriginalUrl))
        {
            ErrorMessage = "Введите URL";
            return Page();
        }

        try
        {
            Guid? ownerId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null)
                    ownerId = Guid.Parse(claim.Value);
            }

            var link = await _service.CreateShortLinkAsync(OriginalUrl, ownerId, ct);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            GeneratedShortUrl = $"{baseUrl}/{link.ShortCode}";
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }
}
