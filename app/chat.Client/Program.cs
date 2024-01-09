using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
/// <inheritdoc/>
public class Program
{
  private static async global::System.Threading.Tasks.Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    await builder.Build().RunAsync();
  }
}