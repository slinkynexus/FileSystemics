using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;
using FileSystemics.SpanIO.Benchmarks.Infrastructure;

namespace FileSystemics.SpanIO.Benchmarks.Allocation;

[Config(typeof(AllocationConfig))]
public class SpanDirectoryAllocationBenchmarks : BenchmarkFileSystemBenchmarkBase {
    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanDirectory_Exists() =>
        SpanDirectory.Exists(FileSystem.ExistingDirectory.AsSpan());

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public string[] SpanDirectory_GetFiles() =>
        SpanDirectory.GetFiles(FileSystem.ExistingDirectory.AsSpan(), EntryPathBuffer.AsSpan());

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public string[] Bcl_GetFiles() =>
        Directory.GetFiles(FileSystem.ExistingDirectory);

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

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public IEnumerable<string> SpanDirectory_EnumerateFiles_Allocating() =>
        SpanDirectory.EnumerateFiles(FileSystem.ExistingDirectory.AsSpan());
}
