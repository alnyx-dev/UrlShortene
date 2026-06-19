using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortLink> ShortLinks => Set<ShortLink>();
    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<ShortLink>()
            .HasOne<User>()
            .WithMany(x => x.ShortLinks)
            .HasForeignKey(x => x.OwnerId)
            .IsRequired(false);

        // Sequence для base62-кодов
        modelBuilder.HasSequence<long>("short_code_sequence").StartsAt(1000);
    }
}