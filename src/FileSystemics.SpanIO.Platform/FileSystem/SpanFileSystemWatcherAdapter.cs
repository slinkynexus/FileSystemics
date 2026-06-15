using FileSystemics.Abstractions;
using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

internal sealed class SpanFileSystemWatcherAdapter : ISpanFileSystemWatcher {
    private readonly FileSystemWatcher _watcher;

    internal SpanFileSystemWatcherAdapter() {
        _watcher = new FileSystemWatcher();
    }

    internal SpanFileSystemWatcherAdapter(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));
        _watcher = new FileSystemWatcher(path.ToString());
    }

    internal SpanFileSystemWatcherAdapter(ReadOnlySpan<char> path, ReadOnlySpan<char> filter) {
        PathArgumentValidation.ValidatePath(path, nameof(path));
        ReadOnlySpan<char> pattern = filter.IsEmpty ? "*".AsSpan() : filter;
        _watcher = new FileSystemWatcher(path.ToString(), pattern.ToString());
    }

    internal SpanFileSystemWatcherAdapter(FileSystemWatcher watcher) {
        _watcher = watcher;
    }

    public void Dispose() => _watcher.Dispose();
}
