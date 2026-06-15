using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace FileSystemics.SpanIO.Benchmarks.Configs;

internal sealed class MicroTimingConfig : ManualConfig {
    public MicroTimingConfig() {
        AddJob(Job.MediumRun
            .WithWarmupCount(8)
            .WithIterationCount(20)
            .WithInvocationCount(128)
            .WithUnrollFactor(16)
            .WithStrategy(RunStrategy.Throughput)
            .WithMaxRelativeError(0.01));

        AddColumn(
            StatisticColumn.Mean,
            StatisticColumn.Error,
            StatisticColumn.StdDev,
            StatisticColumn.Median,
            StatisticColumn.P95,
            StatisticColumn.Min,
            StatisticColumn.Max);

        AddExporter(MarkdownExporter.GitHub, HtmlExporter.Default);
        AddLogger(ConsoleLogger.Default);
    }
}
