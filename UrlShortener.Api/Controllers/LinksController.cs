using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Services;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Api.Controllers;

public record CreateLinkRequest(string Url);

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly ShortLinkService _service;
    private readonly IShortLinkRepository _repository;

    public LinksController(ShortLinkService service, IShortLinkRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLinkRequest request, CancellationToken ct)
    {
        try
        {
            var link = await _service.CreateShortLinkAsync(request.Url, null, ct);
            return Ok(new { shortCode = link.ShortCode, originalUrl = link.OriginalUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly ShortLinkService _service;
    private readonly IShortLinkRepository _repository;

    public RedirectController(ShortLinkService service, IShortLinkRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    [HttpGet("/{code}")]
    public async Task<IActionResult> RedirectToOriginal(string code, CancellationToken ct)
    {
        var link = await _service.ResolveAsync(code, ct);
        if (link == null) return NotFound();

        var click = new ClickEvent
        {
            Id = Guid.NewGuid(),
            ShortLinkId = link.Id,
            Timestamp = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            Referrer = Request.Headers.Referer.ToString()
        };

        _ = _repository.AddClickAsync(click, ct); // не блокируем редирект записью аналитики

        return Redirect(link.OriginalUrl);
    }
}