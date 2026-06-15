using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;
using FileSystemics.SpanIO.Benchmarks.Infrastructure;

namespace FileSystemics.SpanIO.Benchmarks.Allocation;

[Config(typeof(AllocationConfig))]
public class SpanFileAllocationBenchmarks : BenchmarkFileSystemBenchmarkBase {
    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanFile_Exists() =>
        SpanFile.Exists(FileSystem.ExistingFile.AsSpan());

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public bool Bcl_FileExists() =>
        File.Exists(FileSystem.ExistingFile);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public FileAttributes SpanFile_GetAttributes() =>
        SpanFile.GetAttributes(FileSystem.ExistingFile.AsSpan());

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public byte[] SpanFile_ReadAllBytes() =>
        SpanFile.ReadAllBytes(FileSystem.ExistingFile.AsSpan());

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public byte[] Bcl_ReadAllBytes() =>
        File.ReadAllBytes(FileSystem.ExistingFile);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public string SpanFile_ReadAllText() =>
        SpanFile.ReadAllText(FileSystem.ExistingFile.AsSpan());

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public string Bcl_ReadAllText() =>
        File.ReadAllText(FileSystem.ExistingFile);

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public void SpanFile_WriteAllText() =>
        SpanFile.WriteAllText(FileSystem.ExistingFile.AsSpan(), "updated".AsSpan());
}
