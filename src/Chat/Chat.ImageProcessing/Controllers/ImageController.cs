using System.Diagnostics.Metrics;
using Chat.Data;
using Chat.Data.Entities;
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

    public ImageController(ChatDbContext context, ILogger<ImageController> logger, MicroServiceOptions microServiceOptions)
    {
        _context = context;
        _logger = logger;
        this.microServiceOptions = microServiceOptions;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<ChatMessageImage>>> GetImagesForMessage(int id)
    {
        var chatMessageImages = await _context.ChatMessageImages
            .Where(x => x.ChatMessageId == id)
            .ToListAsync();

        return chatMessageImages;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageImage>>> GetAllImages()
    {
        var chatMessageImages = await _context.ChatMessageImages
            .ToListAsync();

        return chatMessageImages;
    }

    [HttpPost("{id}")]
    public async Task<ActionResult> UploadImageForMessage(int id, List<string> images)
    {
        if (microServiceOptions.CompressImages)
        {
            using (var compressionActivity = DiagnosticConfig.ActivitySource.StartActivity("CompressImages"))
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
                    ChatMessageId = id,
                    ImageData = compressedImg,
                    FileName = Guid.NewGuid().ToString(),
                });
                _context.ChatMessageImages.AddRange(chatMessageImages);
            }
        }
        else
        {
            var chatMessageImages = images.Select(img => new ChatMessageImage()
            {
                ChatMessageId = id,
                ImageData = img,
                FileName = Guid.NewGuid().ToString(),
            });
            _context.ChatMessageImages.AddRange(chatMessageImages);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}
