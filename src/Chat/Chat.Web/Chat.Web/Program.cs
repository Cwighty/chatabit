using Chat.Data;
using Chat.Observability;
using Chat.Observability.Options;
using Chat.Web;
using Chat.Web.Client;
using Chat.Web.Client.Options;
using Chat.Web.Components;
using Chat.Web.Services;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();

        var configuration = builder.Configuration;
        ChatApiOptions apiOptions = new();
        configuration.GetRequiredSection(nameof(ChatApiOptions)).Bind(apiOptions);
        builder.Services.AddSingleton(apiOptions);

        builder.Services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddHttpClient("My.ServerAPI", client => client.BaseAddress = new Uri(builder.Configuration["ApiBaseAddress"] ?? throw new Exception("ApiBaseAddress not found in configuration")));
        builder.Services.AddHttpClient("ImageProcessing", client => client.BaseAddress = new Uri(apiOptions.ImageProcessingApiUrl));
        
        builder.Services.AddScoped<IMessageImageService, MessageImageService>();
        
        builder.Services.AddScoped<MessageFetcher>();
        builder.Services.AddScoped(sp => new ChatOptions());

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI"));

        builder.Services.AddControllers();


        builder.AddObservability();

        builder.Services.AddSingleton<UserActivityTracker>();

        var app = builder.Build();

        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Chat.Web.Client.Pages.Home).Assembly);

        app.MapHealthChecks("/health");
        app.MapControllers();

        app.Run();
    }
}
