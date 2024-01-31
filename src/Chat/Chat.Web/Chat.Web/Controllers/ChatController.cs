using System.Diagnostics.Metrics;
using Chat.Data;
using Chat.Data.Entities;
using Chat.Data.Features.Chat;
using Chat.Observability;
using Chat.Observability.Options;
using Chat.Web.Services;
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
    private readonly ChatApiOptions _options;
    private readonly Counter<int> _sentMessages;
    private readonly UserActivityTracker _userActivityTracker;
    private readonly HttpClient _imageProcessingClient;
    private readonly IMessageImageService _messageImageService;

    public ChatController(ChatDbContext context, ILogger<ChatController> logger, Meter meter, ChatApiOptions options, IHttpClientFactory httpClientFactory, IMessageImageService messageImageService)
    {
        _context = context;
        _logger = logger;
        _meter = meter;
        this._options = options;
        _sentMessages = _meter.CreateCounter<int>("chatapi.messages_sent", null, "Number of messages sent");
        _userActivityTracker = new UserActivityTracker(meter);
        _imageProcessingClient = httpClientFactory.CreateClient("ImageProcessing");
        _messageImageService = messageImageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageResponse>>> GetMessages()
    {
        try
        {
            var chatMessages = await _context.ChatMessages
                .ToListAsync();

            var chatMessageImages = await _messageImageService.GetMessages();

            if (chatMessageImages == null)
            {
                throw new Exception("Failed to get images");
            }

            foreach (var chatMessage in chatMessages)
            {
                chatMessage.ChatMessageImages = chatMessageImages
                    .Where(cmi => cmi.ChatMessageId == chatMessage.Id)
                    .ToList();
            }

            return chatMessages.Select(x => x.ToResponseModel()).ToList();
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
        using (var activity = DiagnosticConfig.ChatApiActivitySource.StartActivity("PostMessage"))
        {
            activity?.AddTag("images", request.Images.Count.ToString());
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

                if (request.Images.Count > 0)
                {
                    var response = await _imageProcessingClient.PostAsJsonAsync($"/api/Image/{id}", request.Images);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Failed to upload images");
                    }
                }

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
}
