using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;
using FileSystemics.SpanIO.Benchmarks.Infrastructure;

namespace FileSystemics.SpanIO.Benchmarks.Timing;

[Config(typeof(MicroTimingConfig))]
public class SpanInfoTimingBenchmarks : BenchmarkFileSystemBenchmarkBase {
    private string _filePath = "";
    private string _directoryPath = "";

    [GlobalSetup]
    public void CachePaths() {
        _filePath = FileSystem.ExistingFile;
        _directoryPath = FileSystem.ExistingDirectory;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanFileInfo_Name() {
        SpanFileInfo fileInfo = new(_filePath.AsSpan());
        return fileInfo.Name;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanFileInfo_Exists() {
        SpanFileInfo fileInfo = new(_filePath.AsSpan());
        return fileInfo.Exists;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public long SpanFileInfo_Length() {
        SpanFileInfo fileInfo = new(_filePath.AsSpan());
        return fileInfo.Length;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanDirectoryInfo_Name() {
        SpanDirectoryInfo directoryInfo = new(_directoryPath.AsSpan());
        return directoryInfo.Name;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanDirectoryInfo_Exists() {
        SpanDirectoryInfo directoryInfo = new(_directoryPath.AsSpan());
        return directoryInfo.Exists;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public int SpanBuilder_Append() {
        Span<char> buffer = stackalloc char[128];
        SpanBuilder builder = new(buffer);
        builder.TryAppend("segment".AsSpan());
        builder.TryAppend("/child".AsSpan());
        return builder.Length;
    }
}
