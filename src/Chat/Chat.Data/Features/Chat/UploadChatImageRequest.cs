
namespace Chat.Features.Chat;

public class UploadChatImageRequest
{
    public Guid Id { get; set; }

    public string ImageData { get; set; } = null!;

}
