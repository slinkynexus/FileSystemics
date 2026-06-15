using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;
using FileSystemics.SpanIO.Benchmarks.Infrastructure;

namespace FileSystemics.SpanIO.Benchmarks.Timing;

[Config(typeof(MicroTimingConfig))]
public class SpanDirectoryTimingBenchmarks : BenchmarkFileSystemBenchmarkBase {
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BCL")]
    public bool Bcl_Exists() =>
        Directory.Exists(FileSystem.ExistingDirectory);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanDirectory_Exists() =>
        SpanDirectory.Exists(FileSystem.ExistingDirectory.AsSpan());

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public string[] Bcl_GetFiles() =>
        Directory.GetFiles(FileSystem.ExistingDirectory);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public string[] SpanDirectory_GetFiles() =>
        SpanDirectory.GetFiles(FileSystem.ExistingDirectory.AsSpan(), EntryPathBuffer.AsSpan());

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public int SpanDirectory_EnumerateFiles() {
        int count = 0;
        using SpanDirectoryEntryEnumerator enumerator =
            SpanDirectory.EnumerateFiles(FileSystem.ExistingDirectory.AsSpan(), EntryPathBuffer.AsSpan());
        while (enumerator.MoveNext()) {
            count++;
        }

        return count;
    }
}
