namespace OpenTelemetryWebJobExample;

using System.Diagnostics.Metrics;

public class SampleMeters
{
    public static string Name = "WebJobExampleMeter";
    private readonly Meter _meter;
    public Counter<long> Count { get; }
    public Histogram<long> HistogramWriteDuration { get; }

    public SampleMeters()
    {
        _meter = new Meter(Name);
        Count = _meter.CreateCounter<long>("Count");
        HistogramWriteDuration = _meter.CreateHistogram<long>(
            "Duration",
            description: "Measures the duration.",
            unit: "ms");
    }
}
