using Chat.ImageRedundancy.Options;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static void Main(string[] args)
    {
        // read the configuration from the environment variables
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        // bind to the MicroServiceOptions
        MicroServiceOptions microServiceOptions = new();
        configuration.GetRequiredSection(nameof(MicroServiceOptions)).Bind(microServiceOptions);


        while (true)
        {
            // sleep for the configured interval
            Thread.Sleep(microServiceOptions.SleepInterval * 1000);

            // check for redundant images
            Console.WriteLine("Checking for redundant images");
        }
    }
}
