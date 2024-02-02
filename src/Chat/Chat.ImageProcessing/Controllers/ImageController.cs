using System.Diagnostics.Metrics;
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
    private readonly RedisService _redisService;

    public ImageController(
        ChatDbContext context,
        ILogger<ImageController> logger,
        MicroServiceOptions microServiceOptions,
        RedisService redisService)
    {
        _context = context;
        _logger = logger;
        this.microServiceOptions = microServiceOptions;
        _redisService = redisService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<ChatMessageImage>>> GetImagesForMessage(int id)
    {
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);

        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
        var chatMessageImages = await _context.ChatMessageImages
            .Where(x => x.ChatMessageId == id)
            .ToListAsync();

        return chatMessageImages;
    }

    [HttpGet("file/{id}")]
    public async Task<ActionResult> GetImageFile(int id)
    {
        _logger.LogInformation("Getting image file");
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);

        var chatMessageImage = await _redisService.GetAsync<ChatMessageImage?>($"image:{id}"); ;
        if (chatMessageImage != null)
        {
            _logger.LogInformation("Image found in cache");
            return File(Convert.FromBase64String(chatMessageImage.ImageData), "image/jpeg");
        }
        _logger.LogInformation("Image not found in cache");
        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);

        chatMessageImage = await _context.ChatMessageImages
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (chatMessageImage == null)
        {
            return NotFound();
        }

        await _redisService.SetAsync($"image:{id}", chatMessageImage);
        return File(Convert.FromBase64String(chatMessageImage.ImageData), "image/jpeg");
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

                var chatMessageImages = compressedImages.Select(compressedImg => new ChatMessageImage()
                {
                    ChatMessageId = id, ImageData = compressedImg, FileName = Guid.NewGuid().ToString(),
                });
                _context.ChatMessageImages.AddRange(chatMessageImages);
            }
        }
        else
        {
            var chatMessageImages = images.Select(img => new ChatMessageImage()
            {
                ChatMessageId = id, ImageData = img, FileName = Guid.NewGuid().ToString(),
            });
            _context.ChatMessageImages.AddRange(chatMessageImages);
        }

        Thread.Sleep(microServiceOptions.IntervalTimeSeconds * 1000);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
