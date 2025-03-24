using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static void AddAspireServiceDefaults(this IServiceCollection services)
    {
        services.AddMetrics();
        services.AddOpenTelemetry();
        services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
    }

    public static IServiceCollection ConfigureOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        services
            .AddLogging(logging =>
            {
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                });
            })
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(environment.ApplicationName)
                    .AddSource("Microsoft.AspNetCore.SignalR.Server")
                    .AddAspNetCoreInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        AddOpenTelemetryExporters(services, configuration);

        return services;
    }

    private static void AddOpenTelemetryExporters(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
        );

        if (useOtlpExporter)
        {
            services.AddOpenTelemetry().UseOtlpExporter();
        }
    }

    public static void MapDefaultAspireDevEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");

        endpoints.MapHealthChecks(
            "/alive",
            new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") }
        );
    }
}
