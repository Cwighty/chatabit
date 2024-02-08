namespace Chat.ImageRedundancy.Options;

public class MicroServiceOptions
{
    public int SleepInterval { get; set; } = 5;

    public string ImageProcessingServiceName { get; set; } = "imageprocessing";

}