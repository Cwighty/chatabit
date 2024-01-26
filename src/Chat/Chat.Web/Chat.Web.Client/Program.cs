using Chat.Web.Client;
using Chat.Web.Client.Options;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddHttpClient("My.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI"));

builder.Services.AddScoped<MessageFetcher>();
builder.Services.AddScoped(sp => new ChatOptions());

await builder.Build().RunAsync();
