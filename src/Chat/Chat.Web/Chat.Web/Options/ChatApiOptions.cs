namespace Chat.Observability.Options;

public class ChatApiOptions
{
    public bool CompressImages { get; set; } = true;

    public string ImageProcessingApiUrl { get; set; } = string.Empty;

    public int PollingIntervalMilliseconds { get; set; } = 1000;

}
