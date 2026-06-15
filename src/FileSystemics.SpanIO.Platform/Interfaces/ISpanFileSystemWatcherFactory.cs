using System.Diagnostics.CodeAnalysis;

namespace FileSystemics.Abstractions;

/// <summary>
/// Factory for <see cref="ISpanFileSystemWatcher"/> instances.
/// </summary>
public interface ISpanFileSystemWatcherFactory : ISpanFileSystemEntity {
    /// <summary>Creates a watcher with default settings.</summary>
    ISpanFileSystemWatcher New();

    /// <summary>Creates a watcher for the specified directory.</summary>
    ISpanFileSystemWatcher New(ReadOnlySpan<char> path);

    /// <summary>Creates a watcher for the specified directory and filter.</summary>
    ISpanFileSystemWatcher New(ReadOnlySpan<char> path, ReadOnlySpan<char> filter);

    /// <summary>Wraps an existing <see cref="FileSystemWatcher"/>.</summary>
    [return: NotNullIfNotNull(nameof(fileSystemWatcher))]
    ISpanFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher);
}
