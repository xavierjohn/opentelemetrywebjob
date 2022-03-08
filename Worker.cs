namespace OpenTelemetryWebJobExample;
using System.Diagnostics;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly SampleMeters _sampleMeters;
    static ActivitySource _activitySource = new(SampleTracerSource.Id);
    public Worker(ILogger<Worker> logger, SampleMeters sampleMeters)
    {
        _logger = logger;
        _sampleMeters = sampleMeters;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = _activitySource.StartActivity("Doing somework");
            _sampleMeters.Count.Add(1, new KeyValuePair<string, object?>("work", "foo"));
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await Task.Delay(1000, stoppingToken);

            _sampleMeters.HistogramWriteDuration.Record(stopwatch.ElapsedMilliseconds, tag: KeyValuePair.Create<string, object?>("work", "foo"));
        }
    }
}
