using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence;

public class ShortLinkRepository : IShortLinkRepository
{
    private readonly AppDbContext _context;

    public ShortLinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ShortLink?> GetByCodeAsync(string code, CancellationToken ct)
        => await _context.ShortLinks.SingleOrDefaultAsync(x => x.ShortCode == code, ct);

    public async Task<ShortLink> AddAsync(ShortLink link, CancellationToken ct)
    {
        _context.ShortLinks.Add(link);
        await _context.SaveChangesAsync(ct);
        return link;
    }

    public async Task<long> GetNextSequenceValueAsync(CancellationToken ct)
    {
        var result = await _context.Database
            .SqlQueryRaw<long>("SELECT nextval('short_code_sequence') AS \"Value\"")
            .FirstAsync(ct);
        return result;
    }

    public async Task<IReadOnlyList<ShortLink>> GetByOwnerAsync(Guid ownerId, CancellationToken ct)
        => await _context.ShortLinks
            .Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddClickAsync(ClickEvent click, CancellationToken ct)
    {
        _context.ClickEvents.Add(click);
        await _context.SaveChangesAsync(ct);

        // Денормализованный счётчик для быстрого доступа без JOIN
        var link = await _context.ShortLinks.FindAsync(new object[] { click.ShortLinkId }, ct);
        if (link != null) link.ClicksCount++;
        await _context.SaveChangesAsync(ct);
    }
}