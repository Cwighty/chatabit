namespace Chat.Observability.Options;

public class ChatApiOptions
{
    public bool CompressImages { get; set; } = true;

    public int PollingIntervalMilliseconds { get; set; } = 1000;

}
