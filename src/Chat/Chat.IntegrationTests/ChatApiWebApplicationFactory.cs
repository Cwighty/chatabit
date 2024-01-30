using Chat.Data;
using Chat.Web;
using Chat.Data.Entities;
using Chat.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Testcontainers.PostgreSql;

namespace Chat.IntegrationTests;

public class ChatApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
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

    public ChatApiWebApplicationFactory()
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
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ChatDbContext>));
            services.AddDbContext<ChatDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));
            services.RemoveAll(typeof(IMessageImageService));
            var mockMessageImageService = new Mock<IMessageImageService>();
            mockMessageImageService.Setup(x => x.GetMessages()).ReturnsAsync(new List<ChatMessageImage>());
            services.AddScoped<IMessageImageService>(_ => mockMessageImageService.Object);
        });
    }
}
