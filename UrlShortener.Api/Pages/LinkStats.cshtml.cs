using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Application.DTOs;
using UrlShortener.Application.Services;

namespace UrlShortener.Api.Pages;

[Authorize(AuthenticationSchemes = "Cookies")]
public class LinkStatsModel : PageModel
{
    private readonly ShortLinkService _service;

    public LinkStatsModel(ShortLinkService service)
    {
        _service = service;
    }

    public LinkStatsResponse? Stats { get; set; }
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }
    public string Code { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string code, int? month, int? year, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return RedirectToPage("/Login");

        var userId = Guid.Parse(userIdClaim.Value);
        var now = DateTime.UtcNow;
        CurrentMonth = month ?? now.Month;
        CurrentYear = year ?? now.Year;
        Code = code;

        Stats = await _service.GetLinkStatsAsync(code, userId, CurrentMonth, CurrentYear, ct);
        if (Stats == null) return RedirectToPage("/MyLinks");

        return Page();
    }

    public string MonthName()
    {
        var culture = new CultureInfo("ru-RU");
        return culture.DateTimeFormat.GetMonthName(CurrentMonth);
    }
}
