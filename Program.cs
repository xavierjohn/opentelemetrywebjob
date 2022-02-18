using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryWebJobExample;
using System.Reflection;

var version = Assembly
    .GetExecutingAssembly()
    .GetCustomAttribute<AssemblyFileVersionAttribute>()!
    .Version;
var resourceBuilder = ResourceBuilder
    .CreateEmpty()
    .AddService("WebJobExample", serviceVersion: version, serviceInstanceId: Environment.MachineName);

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((context, builder) =>
    {
        builder.ClearProviders();
        builder.AddConsole();
        if (context.HostingEnvironment.IsDevelopment())
            builder.AddDebug();

        builder.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder)
            .AddConsoleExporter()
            .AddOtlpExporter();
        });
    })
    .ConfigureServices(services =>
    {
        services.AddOpenTelemetryTracing(builder =>
        {
            builder
            .AddSource(SampleTracerSource.Id)
            .SetResourceBuilder(resourceBuilder)
            .SetSampler(new AlwaysOnSampler())
            .AddConsoleExporter()
            .AddOtlpExporter();
        });

        services.AddOpenTelemetryMetrics(options =>
        {
            options
             .SetResourceBuilder(resourceBuilder)
             .AddMeter(SampleMeters.Name)
             .AddOtlpExporter()
             .AddConsoleExporter(options =>
             {
                 // The ConsoleMetricExporter defaults to a manual collect cycle.
                 // This configuration causes metrics to be exported to stdout on a 10s interval.
                 options.MetricReaderType = MetricReaderType.Periodic;
                 options.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
             });

        });
        services.AddSingleton<SampleMeters>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
