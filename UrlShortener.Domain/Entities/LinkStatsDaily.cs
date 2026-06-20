namespace UrlShortener.Domain.Entities;

public class LinkStatsDaily
{
    public Guid Id { get; set; }
    public Guid LinkId { get; set; }
    public DateTime Date { get; set; }
    public int ClicksCount { get; set; }

    public ShortLink Link { get; set; } = null!;
}
