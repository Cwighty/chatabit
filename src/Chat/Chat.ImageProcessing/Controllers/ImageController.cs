using Chat.Data;
using Chat.Data.Entities;
using Chat.ImageProcessing.Services;
using Chat.Observability;
using Chat.Observability.Options;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        IRedisService redisService)
    {
        _logger = logger;
        this.microServiceOptions = microServiceOptions;
        _redisService = redisService;
    }

    [HttpGet("file/{id}")]
    public async Task<ActionResult> GetImageFile(int id)
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
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);

        var filePath = $"./{id}.jpg";
        if (System.IO.File.Exists(filePath))
        {
            _logger.LogInformation("Image found on disk");
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            imageData = Convert.ToBase64String(fileBytes);
            await _redisService.SetAsync($"image:{id}", imageData);
            return File(fileBytes, "image/jpeg");
        }

        return NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageImage>>> GetAllImages()
    {
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);


        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
        var chatMessageImages = await _context.ChatMessageImages
            .ToListAsync();

        return chatMessageImages;
    }

    [HttpPost("{id}")]
    public async Task<ActionResult> UploadImageForMessage(int id, List<string> images)
    {
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
        if (microServiceOptions.CompressImages)
        {
            _logger.LogInformation("Compressing images");
            using (var compressionActivity =
                   DiagnosticConfig.ImageProcessingActivitySource.StartActivity("CompressImages"))
            {
                var compressedImages = new List<string>();
                foreach (var img in images)
                {
                    var imageData = Convert.FromBase64String(img);
                    var stream = new MemoryStream(imageData);

                    var optimizer = new ImageOptimizer();
                    optimizer.Compress(stream);

                    var compressedImage = Convert.ToBase64String(stream.ToArray());
                    compressedImages.Add(compressedImage);
                }

                foreach (var compressedImage in compressedImages)
                {
                    var fileName = Guid.NewGuid().ToString();
                    System.IO.File.WriteAllBytes($"./{fileName}.jpg", Convert.FromBase64String(compressedImage));
                }
            }
        }
        else
        {
            foreach (var img in images)
            {
                var fileName = Guid.NewGuid().ToString();
                System.IO.File.WriteAllBytes($"./{fileName}.jpg", Convert.FromBase64String(img));
            }
        }

        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
