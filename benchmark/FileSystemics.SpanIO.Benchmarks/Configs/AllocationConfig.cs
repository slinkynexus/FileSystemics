using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace FileSystemics.SpanIO.Benchmarks.Configs;

internal sealed class AllocationConfig : ManualConfig {
    public AllocationConfig() {
        AddJob(Job.ShortRun
            .WithGcServer(false)
            .WithGcForce(true)
            .WithIterationCount(5));

        AddDiagnoser(MemoryDiagnoser.Default);
        AddExporter(MarkdownExporter.GitHub, HtmlExporter.Default);
        AddLogger(ConsoleLogger.Default);
    }
}
