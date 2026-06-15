using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;
using FileSystemics.SpanIO.Benchmarks.Infrastructure;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.SpanIO.Benchmarks.Timing;

[Config(typeof(MicroTimingConfig))]
public class SpanFileTimingBenchmarks : BenchmarkFileSystemBenchmarkBase {
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BCL")]
    public bool Bcl_Exists() =>
        File.Exists(FileSystem.ExistingFile);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanFile_Exists() =>
        SpanFile.Exists(FileSystem.ExistingFile.AsSpan());

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public FileAttributes Bcl_GetAttributes() =>
        File.GetAttributes(FileSystem.ExistingFile);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public FileAttributes SpanFile_GetAttributes() =>
        SpanFile.GetAttributes(FileSystem.ExistingFile.AsSpan());

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanFile_OpenHandle() {
        using SafeFileHandle handle = SpanFile.OpenHandle(FileSystem.ExistingFile.AsSpan());
        return !handle.IsInvalid;
    }
}
