using System.Diagnostics.Metrics;
using Chat.Data;
using Chat.Data.Entities;
using Chat.Data.Features.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatDbContext _context;
    private readonly ILogger<ChatController> _logger;
    private readonly Meter _meter;
    private readonly Counter<int> _sentMessages;
    private readonly UserActivityTracker _userActivityTracker;

    public ChatController(ChatDbContext context, ILogger<ChatController> logger, Meter meter)
    {
        _context = context;
        _logger = logger;
        _meter = meter;
        _sentMessages = _meter.CreateCounter<int>("chatapi.messages_sent", null, "Number of messages sent");
        _userActivityTracker = new UserActivityTracker(meter);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageResponse>>> GetMessages()
    {
        try
        {
            var chatMessages = await _context.ChatMessages
                .Include(c => c.ChatMessageImages)
                .ToListAsync();   
            
            var images = chatMessages.SelectMany(x => x.ChatMessageImages.Select(y => y.ImageData));
            
            return chatMessages.Select(x => x.ToResponseModel(images)).ToList();
        }
        catch
        {
            DiagnosticConfig.TrackControllerError(nameof(ChatController), nameof(GetMessages));
            throw;
        }
        finally
        {
            DiagnosticConfig.TrackControllerCall(nameof(ChatController), nameof(GetMessages));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessage>> PostMessage(NewChatMessageRequest request)
    {
        try
        {
            var dbChatMessage = new ChatMessage()
            {
                MessageText = request.MessageText,
                UserName = request.UserName,
                CreatedAt = DateTime.Now,
            };

            await _context.ChatMessages.AddAsync(dbChatMessage);
            await _context.SaveChangesAsync();

            var id = dbChatMessage.Id;

            var chatMessageImages = request.Images.Select(img => new ChatMessageImage()
            {
                ChatMessageId = id,
                ImageData = img, 
                FileName = Guid.NewGuid().ToString(),
            });

            _context.ChatMessageImages.AddRange(chatMessageImages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Message posted by {UserName} at {CreatedAt}", dbChatMessage.UserName, dbChatMessage.CreatedAt);
            _sentMessages.Add(1);

            _userActivityTracker.TrackUserActivity(dbChatMessage);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting message");
            DiagnosticConfig.TrackControllerError(nameof(ChatController), nameof(PostMessage));
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        finally
        {
            DiagnosticConfig.TrackControllerCall(nameof(ChatController), nameof(PostMessage));
        }
    }
}
