using FileSystemics.Abstractions;
using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

internal sealed class SpanFileInfoFactoryAdapter(ISpanFileSystem fileSystem) : ISpanFileInfoFactory {
    public ISpanFileSystem FileSystem { get; } = fileSystem;

    public ISpanFileInfo New(ReadOnlySpan<char> fileName) {
        PathArgumentValidation.ValidatePath(fileName, nameof(fileName));
        return new SpanFileInfoAdapter(FileSystem, fileName);
    }

    public ISpanFileInfo? Wrap(FileInfo? fileInfo) =>
        fileInfo is null ? null : new SpanFileInfoAdapter(FileSystem, fileInfo.FullName);
}
