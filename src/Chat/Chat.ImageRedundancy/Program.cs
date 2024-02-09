using Chat.Data;
using Chat.ImageRedundancy;
using Chat.ImageRedundancy.Options;
using Chat.Observability;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        
        builder.Services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Singleton);

        builder.AddMicroServiceOptions();
        builder.AddObservability();

        var host = builder.Build();


        host.Run();
    }
}
