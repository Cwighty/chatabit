namespace Chat.Observability.Options;

public class ChatApiOptions
{
    public string ImageProcessingApiUrl { get; set; } = string.Empty;

    public int PollingIntervalMilliseconds { get; set; } = 1000;

}
