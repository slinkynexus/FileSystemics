using FileSystemics.Abstractions;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO;

internal sealed class SpanFileStreamFactoryAdapter(ISpanFileSystem fileSystem) : ISpanFileStreamFactory {
    public ISpanFileSystem FileSystem { get; } = fileSystem;

    public Stream New(SafeFileHandle handle, FileAccess access) =>
        new FileStream(handle, access);

    public Stream New(SafeFileHandle handle, FileAccess access, int bufferSize) =>
        new FileStream(handle, access, bufferSize);

    public Stream New(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) =>
        new FileStream(handle, access, bufferSize, isAsync);

    public Stream New(ReadOnlySpan<char> path, FileMode mode) =>
        New(path, mode, FileAccess.ReadWrite, FileShare.None);

    public Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access) =>
        New(path, mode, access, FileShare.None);

    public Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share) =>
        New(path, mode, access, share, bufferSize: 4096);

    public Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, int bufferSize) =>
        New(path, mode, access, share, bufferSize, useAsync: false);

    public Stream New(
        ReadOnlySpan<char> path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        int bufferSize,
        bool useAsync) {
        FileOptions options = useAsync ? FileOptions.Asynchronous : FileOptions.None;
        return New(path, mode, access, share, bufferSize, options);
    }

    public Stream New(
        ReadOnlySpan<char> path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        int bufferSize,
        FileOptions options) =>
        new FileStream(
            SpanFile.OpenHandle(path, mode, access, share, options),
            access,
            bufferSize,
            (options & FileOptions.Asynchronous) != 0);

    public Stream New(ReadOnlySpan<char> path, FileStreamOptions options) =>
        new FileStream(
            SpanFile.OpenHandle(path, options.Mode, options.Access, options.Share, options.Options),
            options.Access,
            options.BufferSize,
            (options.Options & FileOptions.Asynchronous) != 0);

    public Stream Wrap(FileStream fileStream) => fileStream;
}
