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
            var link = await _service.CreateShortLinkAsync(OriginalUrl, null, ct);
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