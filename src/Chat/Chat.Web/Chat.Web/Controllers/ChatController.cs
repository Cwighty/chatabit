using System.Diagnostics.Metrics;
using Chat.Data;
using Chat.Data.Entities;
using Chat.Data.Features.Chat;
using Chat.Features.Chat;
using Chat.Observability;
using Chat.Observability.Options;
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

    public ChatController(ChatDbContext context, ILogger<ChatController> logger, Meter meter, ChatApiOptions options, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _meter = meter;
        this._options = options;
        _sentMessages = _meter.CreateCounter<int>("chatapi.messages_sent", null, "Number of messages sent");
        _userActivityTracker = new UserActivityTracker(meter);
        _imageProcessingClient = httpClientFactory.CreateClient("ImageProcessing");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageResponse>>> GetMessages()
    {
        try
        {
            var chatMessages = await _context.ChatMessages
                .Include(x => x.ChatMessageImages)
                .ToListAsync();

            return chatMessages
                    .TakeLast(10)
                    .Select(x => x.ToResponseModel()).ToList();
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
                    Id = Guid.NewGuid(),
                    MessageText = request.MessageText,
                    UserName = request.UserName,
                    CreatedAt = DateTime.Now,
                };

                await _context.ChatMessages.AddAsync(dbChatMessage);
                await _context.SaveChangesAsync();

                if (request.Images.Count > 0)
                {
                    var chatMessageImages = request.Images.Select(imgData => new UploadChatImageRequest()
                    {
                        ChatMessageId = dbChatMessage.Id,
                        Id = Guid.NewGuid(),
                        ImageData = imgData
                    });

                    var response = await _imageProcessingClient.PostAsJsonAsync($"/api/Image/", chatMessageImages);
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
