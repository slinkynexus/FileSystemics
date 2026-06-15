using FileSystemics.Abstractions;

namespace FileSystemics.IO;

internal sealed class SpanFileSystemWatcherFactoryAdapter(ISpanFileSystem fileSystem) : ISpanFileSystemWatcherFactory {
    public ISpanFileSystem FileSystem { get; } = fileSystem;

    public ISpanFileSystemWatcher New() => new SpanFileSystemWatcherAdapter();

    public ISpanFileSystemWatcher New(ReadOnlySpan<char> path) => new SpanFileSystemWatcherAdapter(path);

    public ISpanFileSystemWatcher New(ReadOnlySpan<char> path, ReadOnlySpan<char> filter) =>
        new SpanFileSystemWatcherAdapter(path, filter);

    public ISpanFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher) =>
        fileSystemWatcher is null ? null : new SpanFileSystemWatcherAdapter(fileSystemWatcher);
}
