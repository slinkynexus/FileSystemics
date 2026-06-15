using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;
using FileSystemics.SpanIO.Benchmarks.Infrastructure;
using IOPath = System.IO.Path;

namespace FileSystemics.SpanIO.Benchmarks.Timing;

[Config(typeof(MicroTimingConfig))]
public class SpanPathTimingBenchmarks {
    private readonly char[] _destination = new char[BenchmarkPaths.DestinationCapacity];

    [ParamsAllValues]
    public PathSize Size { get; set; }

    private ReadOnlySpan<char> RelativePath => BenchmarkPaths.GetRelative(Size);

    private string RelativePathString => BenchmarkPaths.GetRelativeString(Size);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BCL")]
    public ReadOnlySpan<char> Bcl_GetFileName() =>
        IOPath.GetFileName(RelativePathString).AsSpan();

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanPath_GetFileName() =>
        SpanPath.GetFileName(RelativePath);

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public ReadOnlySpan<char> Bcl_GetDirectoryName() =>
        IOPath.GetDirectoryName(RelativePathString).AsSpan();

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanPath_GetDirectoryName() =>
        SpanPath.GetDirectoryName(RelativePath);

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public ReadOnlySpan<char> Bcl_GetExtension() =>
        IOPath.GetExtension(RelativePathString).AsSpan();

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanPath_GetExtension() =>
        SpanPath.GetExtension(RelativePath);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanPath_GetPathRoot() =>
        SpanPath.GetPathRoot(RelativePath);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanPath_IsPathRooted() =>
        SpanPath.IsPathRooted(RelativePath);

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public string Bcl_Combine() =>
        IOPath.Combine("root", RelativePathString);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanPath_TryCombine() {
        Span<char> destination = _destination;
        return SpanPath.TryCombine("root".AsSpan(), RelativePath, destination, out _);
    }

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public string Bcl_GetRelativePath() =>
        IOPath.GetRelativePath("/base/dir".AsSpan().ToString(), RelativePathString);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanPath_TryGetRelativePath() {
        Span<char> destination = _destination;
        return SpanPath.TryGetRelativePath("/base/dir".AsSpan(), RelativePath, destination, out _);
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanPath_TryChangeExtension() {
        Span<char> destination = _destination;
        return SpanPath.TryChangeExtension(RelativePath, ".json".AsSpan(), destination, out _);
    }
}
