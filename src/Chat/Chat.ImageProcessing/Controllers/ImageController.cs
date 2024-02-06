using Chat.Data;
using Chat.Data.Entities;
using Chat.Features.Chat;
using Chat.ImageProcessing.Services;
using Chat.Observability;
using Chat.Observability.Options;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly ChatDbContext _context;
    private readonly ILogger<ImageController> _logger;
    private readonly MicroServiceOptions microServiceOptions;
    private readonly IRedisService _redisService;

    public ImageController(
        ILogger<ImageController> logger,
        MicroServiceOptions microServiceOptions,
        ChatDbContext context,
        IRedisService redisService)
    {
        _logger = logger;
        this.microServiceOptions = microServiceOptions;
        _context = context;
        _redisService = redisService;
    }

    [HttpGet("file/{id}")]
    public async Task<ActionResult> GetImageFile(Guid id)
    {
        _logger.LogInformation("Getting image file");
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);

        var imageData = await _redisService.GetAsync<string?>($"image:{id}"); ;
        if (imageData != null)
        {
            _logger.LogInformation("Image found in cache");
            return File(Convert.FromBase64String(imageData), "image/jpeg");
        }
        _logger.LogInformation("Image not found in cache");

        var imageOwnerIdentifier = _context.ImageLocations.FirstOrDefault(x => x.ChatMessageImageId == id);
        if (imageOwnerIdentifier == null)
        {
            _logger.LogInformation("Image not found on any server");
            return NotFound();
        }

        if (imageOwnerIdentifier.ServiceIdentifier == microServiceOptions.Identifier)
        {
            _logger.LogInformation("Image found on this server");
            var filePath = $"{microServiceOptions.ImageDirectory}/{id}.jpg";
            if (System.IO.File.Exists(filePath))
            {
                _logger.LogInformation("Image found on disk");
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                imageData = Convert.ToBase64String(fileBytes);
                await _redisService.SetAsync($"image:{id}", imageData);
                return File(fileBytes, "image/jpeg");
            }
        }
        else
        {
            var serviceName = $"http://imageprocessing{imageOwnerIdentifier.ServiceIdentifier}:8080";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{serviceName}/api/image/file/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Image found on another server");
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                return File(fileBytes, "image/jpeg");
            }
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> UploadImageForMessage(List<UploadChatImageRequest> uploadRequest)
    {
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
        if (microServiceOptions.CompressImages)
        {
            _logger.LogInformation("Compressing images");
            using (var compressionActivity =
                   DiagnosticConfig.ImageProcessingActivitySource.StartActivity("CompressImages"))
            {
                var compressedImages = new List<string>();
                foreach (var img in uploadRequest)
                {
                    var imageData = Convert.FromBase64String(img.ImageData);
                    var stream = new MemoryStream(imageData);

                    var optimizer = new ImageOptimizer();
                    optimizer.Compress(stream);

                    var compressedImage = Convert.ToBase64String(stream.ToArray());
                    Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
                    System.IO.File.WriteAllBytes($"{microServiceOptions.ImageDirectory}/{img.Id}.jpg", Convert.FromBase64String(compressedImage));
                    var imageReference = new ImageLocation
                    {
                        Id = Guid.NewGuid(),
                        ChatMessageImageId = img.Id,
                        ServiceIdentifier = microServiceOptions.Identifier
                    };
                    _context.ImageLocations.Add(imageReference);
                    await _context.SaveChangesAsync();
                }
            }
        }
        else
        {
            foreach (var img in uploadRequest)
            {
                Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
                System.IO.File.WriteAllBytes($"{microServiceOptions.ImageDirectory}/{img.Id}.jpg", Convert.FromBase64String(img.ImageData));
            }
        }

        return Ok();
    }
}
