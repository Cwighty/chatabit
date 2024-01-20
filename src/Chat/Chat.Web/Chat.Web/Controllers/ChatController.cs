using System.Diagnostics.Metrics;
using Chat.Data;
using Chat.Data.Entities;
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
    public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages()
    {
        return await _context.ChatMessages.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessage>> PostMessage(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Message posted by {UserName} at {CreatedAt}", message.UserName, message.CreatedAt);
        _sentMessages.Add(1);
        
        _userActivityTracker.TrackUserActivity(message);

        return Created();
    }
}
