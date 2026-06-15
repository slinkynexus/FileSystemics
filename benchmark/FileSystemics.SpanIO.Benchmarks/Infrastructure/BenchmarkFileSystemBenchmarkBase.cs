using BenchmarkDotNet.Attributes;
using FileSystemics.IO;

namespace FileSystemics.SpanIO.Benchmarks.Infrastructure;

public abstract class BenchmarkFileSystemBenchmarkBase {
    protected BenchmarkFileSystem FileSystem = null!;
    protected char[] EntryPathBuffer = [];

    [GlobalSetup]
    public void Setup() {
        FileSystem = new BenchmarkFileSystem();
        FileSystem.Create();
        EntryPathBuffer = GC.AllocateUninitializedArray<char>(
            SpanDirectory.GetEntryPathBufferCapacity(FileSystem.ExistingDirectory.AsSpan()));
    }

    [GlobalCleanup]
    public void Cleanup() => FileSystem.Dispose();
}
