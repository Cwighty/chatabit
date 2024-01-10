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

  public ChatController(ChatDbContext context)
  {
    _context = context;
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

    return Created();
  }
}