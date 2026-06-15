namespace FileSystemics.Abstractions;

/// <summary>
/// Root facade for span-first path, file, and directory operations.
/// </summary>
public interface ISpanFileSystem {
    /// <summary>Span-based directory APIs.</summary>
    ISpanDirectory Directory { get; }

    /// <summary>Factory for directory metadata instances.</summary>
    ISpanDirectoryInfoFactory DirectoryInfo { get; }

    /// <summary>Factory for drive metadata instances.</summary>
    ISpanDriveInfoFactory DriveInfo { get; }

    /// <summary>Span-based file APIs.</summary>
    ISpanFile File { get; }

    /// <summary>Factory for file metadata instances.</summary>
    ISpanFileInfoFactory FileInfo { get; }

    /// <summary>Factory for file stream instances.</summary>
    ISpanFileStreamFactory FileStream { get; }

    /// <summary>Factory for filesystem watcher instances.</summary>
    ISpanFileSystemWatcherFactory FileSystemWatcher { get; }

    /// <summary>Factory for file version information.</summary>
    ISpanFileVersionInfoFactory FileVersionInfo { get; }

    /// <summary>Span-based path manipulation APIs.</summary>
    ISpanPath Path { get; }
}
