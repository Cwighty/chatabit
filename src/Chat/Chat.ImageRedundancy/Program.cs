using Chat.ImageRedundancy;
using Chat.ImageRedundancy.Options;
using Chat.Observability;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        builder.AddMicroServiceOptions();
        builder.AddObservability();

        var host = builder.Build();


        host.Run();
    }
}