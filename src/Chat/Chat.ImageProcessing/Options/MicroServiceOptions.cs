﻿namespace Chat.Observability.Options;

public class MicroServiceOptions
{
    public bool CompressImages { get; set; } = true;

    public int IntervalTimeSeconds { get; set; } = 1;

}

public static class MicroServiceOptionsRegistration
{

    public static WebApplicationBuilder AddMicroServiceOptions(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        MicroServiceOptions microServiceOptions = new();
        configuration.GetRequiredSection(nameof(MicroServiceOptions)).Bind(microServiceOptions);
        builder.Services.AddSingleton(microServiceOptions);
        return builder;
    }
}
