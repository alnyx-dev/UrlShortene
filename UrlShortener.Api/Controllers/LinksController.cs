using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.DTOs;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Api.Controllers;

public record CreateLinkRequest(string Url);

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly ShortLinkService _service;

    public LinksController(ShortLinkService service)
    {
        _service = service;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateLinkRequest request, CancellationToken ct)
    {
        try
        {
            var link = await _service.CreateShortLinkAsync(request.Url, null, ct);
            return Ok(new ShortLinkResponse(link.ShortCode, link.OriginalUrl, link.CreatedAt, link.ExpiresAt, link.ClicksCount));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("claim")]
    [Authorize]
    public async Task<IActionResult> CreateClaimed([FromBody] CreateLinkRequest request, CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            var link = await _service.CreateShortLinkAsync(request.Url, userId, ct);
            return Ok(new ShortLinkResponse(link.ShortCode, link.OriginalUrl, link.CreatedAt, link.ExpiresAt, link.ClicksCount));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyLinks(CancellationToken ct)
    {
        var userId = GetUserId();
        var links = await _service.GetMyLinksAsync(userId, ct);
        var response = links
            .Select(l => new ShortLinkResponse(l.ShortCode, l.OriginalUrl, l.CreatedAt, l.ExpiresAt, l.ClicksCount))
            .ToList();
        return Ok(response);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!.Value);
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

        _ = _repository.AddClickAsync(click, ct);

        return Redirect(link.OriginalUrl);
    }
}
