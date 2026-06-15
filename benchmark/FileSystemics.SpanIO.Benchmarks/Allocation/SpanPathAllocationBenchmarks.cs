using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;
using FileSystemics.SpanIO.Benchmarks.Infrastructure;
using IOPath = System.IO.Path;

namespace FileSystemics.SpanIO.Benchmarks.Allocation;

[Config(typeof(AllocationConfig))]
public class SpanPathAllocationBenchmarks {
    private readonly char[] _destination = new char[BenchmarkPaths.DestinationCapacity];

    private ReadOnlySpan<char> Left => "root/left".AsSpan();

    private ReadOnlySpan<char> Right => BenchmarkPaths.MediumRelative.AsSpan();

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanPath_TryJoin_ZeroAlloc() {
        Span<char> destination = _destination;
        return SpanPath.TryJoin(Left, Right, destination, out _);
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanPath_Combine_ZeroAlloc() {
        Span<char> destination = _destination;
        return SpanPath.Combine(Left, Right, destination);
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanPath_GetRelativePath_ZeroAlloc() {
        Span<char> destination = _destination;
        return SpanPath.GetRelativePath("/base".AsSpan(), Right, destination);
    }

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public string Bcl_PathJoin() =>
        IOPath.Join(Left, Right);
}
