using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.Stats;

public class StatsAggregationService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<StatsAggregationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public StatsAggregationService(IServiceProvider sp, ILogger<StatsAggregationService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("StatsAggregationService started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await AggregateAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Aggregation failed");
            }

            await Task.Delay(_interval, ct);
        }
    }

    private async Task AggregateAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var from = DateTime.UtcNow.AddDays(-2);

        var affected = await ctx.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""DailyStats"" (""Id"", ""LinkId"", ""Date"", ""ClicksCount"")
            SELECT gen_random_uuid(), ce.""ShortLinkId"", DATE(ce.""Timestamp""), COUNT(*)
            FROM ""ClickEvents"" ce
            WHERE ce.""Timestamp"" >= {0}
            GROUP BY ce.""ShortLinkId"", DATE(ce.""Timestamp"")
            ON CONFLICT (""LinkId"", ""Date"") 
            DO UPDATE SET ""ClicksCount"" = EXCLUDED.""ClicksCount""
        ", from, ct);

        _logger.LogDebug("Aggregated {Rows} daily stat rows", affected);
    }
}
