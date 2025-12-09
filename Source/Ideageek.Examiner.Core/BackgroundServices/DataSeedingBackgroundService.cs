using Ideageek.Examiner.Core.DataSeeder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ideageek.Examiner.Core.BackgroundServices;

public class DataSeedingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeedingBackgroundService> _logger;

    public DataSeedingBackgroundService(IServiceProvider serviceProvider, ILogger<DataSeedingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data seeding background service failed");
            throw;
        }
    }
}
