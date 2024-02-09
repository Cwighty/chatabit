namespace Chat.ImageRedundancy.Options;

public class MicroServiceOptions
{
   public int SleepInterval { get; set; } = 5;

   public string ImageProcessingServiceName { get; set; } = "imageprocessing"; 
}

public static class MicroServiceOptionsRegistration
{

    public static HostApplicationBuilder AddMicroServiceOptions(this HostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        MicroServiceOptions microServiceOptions = new();
        configuration.GetRequiredSection(nameof(MicroServiceOptions)).Bind(microServiceOptions);
        builder.Services.AddSingleton(microServiceOptions);
        return builder;
    }
}
