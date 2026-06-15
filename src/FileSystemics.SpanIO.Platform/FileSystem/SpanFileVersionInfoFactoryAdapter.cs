using FileSystemics.Abstractions;

namespace FileSystemics.IO;

internal sealed class SpanFileVersionInfoFactoryAdapter(ISpanFileSystem fileSystem) : ISpanFileVersionInfoFactory {
    public ISpanFileSystem FileSystem { get; } = fileSystem;

    public ISpanFileVersionInfo GetVersionInfo(ReadOnlySpan<char> fileName) =>
        new SpanFileVersionInfoAdapter(fileName);
}
