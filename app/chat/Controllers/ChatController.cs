using Chat.Data;
using Chat.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chat.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
  private readonly ChatDbContext _context;

  public ChatController(ChatDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
  {
    return await _context.Messages.ToListAsync();
  }

  [HttpPost]
  public async Task<ActionResult<Message>> PostMessage(Message message)
  {
    _context.Messages.Add(message);
    await _context.SaveChangesAsync();

    return Created();
  }
}