using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortLink> ShortLinks => Set<ShortLink>();
    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();
    public DbSet<LinkStatsDaily> DailyStats => Set<LinkStatsDaily>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<ShortLink>()
            .HasIndex(x => x.ShortCode)
            .IsUnique();

        modelBuilder.Entity<ShortLink>()
            .HasOne<User>()
            .WithMany(x => x.ShortLinks)
            .HasForeignKey(x => x.OwnerId)
            .IsRequired(false);

        modelBuilder.Entity<LinkStatsDaily>()
            .HasIndex(x => new { x.LinkId, x.Date })
            .IsUnique();

        modelBuilder.Entity<LinkStatsDaily>()
            .HasOne(x => x.Link)
            .WithMany()
            .HasForeignKey(x => x.LinkId);

        // Sequence для base62-кодов
        modelBuilder.HasSequence<long>("short_code_sequence").StartsAt(1000);
    }
}