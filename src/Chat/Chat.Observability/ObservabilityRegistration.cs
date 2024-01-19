using System.Diagnostics;
using System.Diagnostics.Metrics;
using Chat.Observability.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using Serilog.Settings.Configuration;
using Serilog.Sinks.OpenTelemetry;

namespace Chat.Observability;

public static class ObservabilityRegistration
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        var configuration = builder.Configuration;

        ObservabilityOptions observabilityOptions = new();

        configuration.GetRequiredSection(nameof(ObservabilityOptions)).Bind(observabilityOptions);

        builder.Host.AddSerilog();

        builder
            .Services.AddOpenTelemetry()
            .AddTracing(observabilityOptions)
            .AddMetrics(observabilityOptions);

        return builder;
    }

    private static OpenTelemetryBuilder AddTracing(
        this OpenTelemetryBuilder builder,
        ObservabilityOptions observabilityOptions
    )
    {
        builder.WithTracing(tracing =>
        {
            tracing
                .AddSource(observabilityOptions.ServiceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService(observabilityOptions.ServiceName)
                )
                .SetErrorStatusOnException()
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                });

            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint = observabilityOptions.CollectorUri;
                options.ExportProcessorType = ExportProcessorType.Batch;
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        });

        return builder;
    }

    private static OpenTelemetryBuilder AddMetrics(
        this OpenTelemetryBuilder builder,
        ObservabilityOptions observabilityOptions
    )
    {
        builder.WithMetrics(metrics =>
        {
            var meter = new Meter(observabilityOptions.ServiceName);

            metrics
                .AddMeter(meter.Name)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(meter.Name))
                .AddAspNetCoreInstrumentation();

            metrics.AddOtlpExporter(options =>
            {
                options.Endpoint = observabilityOptions.CollectorUri;
                options.ExportProcessorType = ExportProcessorType.Batch;
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        });

        return builder;
    }

    public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog(
            (context, provider, options) =>
            {
                var environment = context.HostingEnvironment.EnvironmentName;
                var configuration = context.Configuration;

                ObservabilityOptions observabilityOptions = new();

                configuration.GetSection(nameof(ObservabilityOptions)).Bind(observabilityOptions);

                var configOptions = new ConfigurationReaderOptions()
                {
                    SectionName = $"{nameof(ObservabilityOptions)}:{nameof(ObservabilityOptions)}:Serilog",
                };

                options
                    .ReadFrom.Configuration(context.Configuration, configOptions)
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironment(environment)
                    .Enrich.WithMachineName()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithDemystifiedStackTraces()
                    .Enrich.WithProperty("ApplicationName", observabilityOptions.ServiceName);

                options.WriteTo.OpenTelemetry(cfg =>
                {
                    cfg.Endpoint = $"{observabilityOptions.CollectorUrl}/v1/logs";
                    cfg.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField;
                    cfg.ResourceAttributes = new Dictionary<string, object>
                    {
                        { "service.name", observabilityOptions.ServiceName },
                    };
                });
            }
        );
        return hostBuilder;
    }

    public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder loggingBuilder, ObservabilityOptions observabilityOptions)
    {
        loggingBuilder.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.IncludeFormattedMessage = true;
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(observabilityOptions.ServiceName))
            .AddOtlpExporter(otlp =>
            {
                otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                otlp.Endpoint = observabilityOptions.CollectorUri;
            });
        });
        return loggingBuilder;
    }
}
