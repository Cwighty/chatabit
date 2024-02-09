using System.Diagnostics;
using Chat.ImageRedundancy.Options;
using Chat.Observability;

namespace Chat.ImageRedundancy;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MicroServiceOptions _microServiceOptions;

    public Worker(ILogger<Worker> logger, MicroServiceOptions microServiceOptions)
    {
        _logger = logger;
        _microServiceOptions = microServiceOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = DiagnosticConfig.ImageProcessingActivitySource.StartActivity("ImageRedundancyCheck");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking for image redundancy at: {time}", DateTimeOffset.Now);
            await Task.Delay(_microServiceOptions.SleepInterval * 1000, stoppingToken);
        }
    }
}
