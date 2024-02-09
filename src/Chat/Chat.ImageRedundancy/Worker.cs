using System.Diagnostics;
using Chat.Data;
using Chat.ImageRedundancy.Options;
using Chat.Observability;
using Microsoft.EntityFrameworkCore;

namespace Chat.ImageRedundancy;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MicroServiceOptions _microServiceOptions;
    private readonly ChatDbContext _dbContext;

    public Worker(ILogger<Worker> logger, MicroServiceOptions microServiceOptions, ChatDbContext dbContext)
    {
        _logger = logger;
        _microServiceOptions = microServiceOptions;
        _dbContext = dbContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = DiagnosticConfig.ImageProcessingActivitySource.StartActivity("ImageRedundancyCheck");
        DiagnosticConfig.TrackImageRedundancyNonRedundant(GetNonRedundantImageCount);
        DiagnosticConfig.TrackImageRedundancyUploadTotal(GetTotalImageCount);
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking for image redundancy at: {time}", DateTimeOffset.Now);
            await Task.Delay(_microServiceOptions.SleepInterval * 1000, stoppingToken);

            // Check for redundant images
            var imageLocations = await _dbContext.ImageLocations
                .ToListAsync(stoppingToken);
            var nonRedundantImages = imageLocations
                .GroupBy(il => il.ChatMessageImageId)
                .Where(g => g.Count() == 1)
                .SelectMany(g => g)
                .ToList();

            if (nonRedundantImages.Count == 0)
            {
                _logger.LogInformation("Redundancy achieved");
                continue;
            }

            _logger.LogInformation(nonRedundantImages.Count + " non-redundant images found");

            var serviceIds = Enumerable.Range(1, _microServiceOptions.ImageProcessingServiceCount).ToList();
            foreach (var nri in nonRedundantImages)
            {
                // Pick a random service not equal to the current service
                var otherServices = serviceIds.Where(s => s != nri.ServiceIdentifier).ToList();

                var randomServiceId = otherServices[new Random().Next(otherServices.Count)];

                var imageHttpClient = new HttpClient();

                var serviceAddress = $"http://imageprocessing{randomServiceId}:8080/api/image/";

                await imageHttpClient.PostAsync(serviceAddress + "fetch-missing-image/" + nri.ChatMessageImageId, null, stoppingToken);
            }
        }
    }

    private int GetTotalImageCount()
    {
        // number of unique images
        return _dbContext.ImageLocations
            .GroupBy(il => il.ChatMessageImageId)
            .Count();
    }

    private int GetNonRedundantImageCount()
    {
        // number of unique images that are not redundant
        var imageLocations = _dbContext.ImageLocations
                .ToList();

        return imageLocations
            .GroupBy(il => il.ChatMessageImageId)
            .Where(g => g.Count() == 1)
            .SelectMany(g => g)
            .Count();
    }
}
