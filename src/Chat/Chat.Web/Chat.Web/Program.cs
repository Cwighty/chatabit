using Chat.Data;
using Chat.Observability;
using Chat.Observability.Options;
using Chat.Web.Client;
using Chat.Web.Client.Options;
using Chat.Web.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;

namespace Chat.Web;

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

        builder.Services.AddScoped<MessageFetcher>();
        builder.Services.AddScoped(sp => new ChatOptions());

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI"));

        builder.Services.AddScoped<HubConnection>(sp =>
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl("http://signalrserver:8080/api/chatHub")
                .WithAutomaticReconnect()
                .Build();
            hubConnection.StartAsync().Wait();
            return hubConnection;
        });

        builder.Services.AddScoped<LocalStorageAccessor>();

        builder.Services.AddControllers();

        builder.Services.AddSignalR();

        builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowAll",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });


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

            app.UseCors("AllowAll");
            app.MapGet("/api/Image/{**rest}", async context =>
                {
                    var httpClient = new HttpClient();
                    var imageUrl = $"http://imageprocessing:8080{context.Request.Path.Value}";
                    var response = await httpClient.GetAsync(imageUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStreamAsync();
                        context.Response.ContentType = response.Content.Headers.ContentType.ToString();
                        await content.CopyToAsync(context.Response.Body);
                    }
                });
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
