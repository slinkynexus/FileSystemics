using FileSystemics.Abstractions;
using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

internal sealed class SpanDirectoryInfoFactoryAdapter(ISpanFileSystem fileSystem) : ISpanDirectoryInfoFactory {
    public ISpanFileSystem FileSystem { get; } = fileSystem;

    public ISpanDirectoryInfo New(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));
        return new SpanDirectoryInfoAdapter(FileSystem, path);
    }

    public ISpanDirectoryInfo? Wrap(DirectoryInfo? directoryInfo) =>
        directoryInfo is null ? null : new SpanDirectoryInfoAdapter(FileSystem, directoryInfo.FullName);
}
