using Chat.Data;
using Chat.Data.Entities;
using Chat.ImageProcessing;
using Chat.ImageProcessing.Services;
using Chat.Observability.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Testcontainers.PostgreSql;

namespace Chat.IntegrationTests;

public class ImageProcessingWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static string FindProjectRootByMarker()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        directory = directory?.Parent;
        return directory?.FullName ?? throw new Exception("Project root could not be located.");
    }

    private readonly PostgreSqlContainer _dbContainer;

    public ImageProcessingWebApplicationFactory()
    {
        var directory = FindProjectRootByMarker() + "/ChatDatabase/init-scripts";
        _dbContainer = new PostgreSqlBuilder()
           .WithImage("postgres")
           .WithPassword("Strong_password_123!")
           .WithResourceMapping(new DirectoryInfo(directory), "/docker-entrypoint-initdb.d")
           .WithCleanUp(true)
           .WithAutoRemove(true)
           .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();

        // delete all jpg files in the TestUploads directory 
        var directory = FindProjectRootByMarker() + "/Chat/Chat.IntegrationTests/TestUploads";
        foreach (var file in Directory.EnumerateFiles(directory, "*.jpg"))
        {
            File.Delete(file);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ChatDbContext>));
            services.AddDbContext<ChatDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));
            services.RemoveAll<IRedisService>();
            var mockRedisService = new Mock<IRedisService>();
            mockRedisService.Setup(x => x.GetAsync<ChatMessageImage?>(It.IsAny<string>())).ReturnsAsync((ChatMessageImage?)null);
            services.AddScoped<IRedisService>(_ => mockRedisService.Object);

            services.RemoveAll<MicroServiceOptions>();
            services.AddScoped(options =>
            new MicroServiceOptions
            {
                Identifier = 1,
                IntervalTimeSeconds = 1,
                ImageDirectory = "../../../TestUploads",
            });
        });
    }
}
