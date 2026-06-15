using Microsoft.Win32.SafeHandles;

namespace FileSystemics.Abstractions;

/// <summary>
/// Factory for file streams in a span filesystem abstraction.
/// </summary>
public interface ISpanFileStreamFactory : ISpanFileSystemEntity {
    /// <summary>Creates a stream from an existing file handle.</summary>
    Stream New(SafeFileHandle handle, FileAccess access);

    /// <summary>Creates a stream from an existing file handle with a buffer size.</summary>
    Stream New(SafeFileHandle handle, FileAccess access, int bufferSize);

    /// <summary>Creates a stream from an existing file handle with async support.</summary>
    Stream New(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync);

    /// <summary>Opens a file stream with the specified mode.</summary>
    Stream New(ReadOnlySpan<char> path, FileMode mode);

    /// <summary>Opens a file stream with the specified mode and access.</summary>
    Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access);

    /// <summary>Opens a file stream with the specified mode, access, and share.</summary>
    Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share);

    /// <summary>Opens a file stream with the specified mode, access, share, and buffer size.</summary>
    Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, int bufferSize);

    /// <summary>Opens a file stream with the specified mode, access, share, buffer size, and async flag.</summary>
    Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync);

    /// <summary>Opens a file stream with the specified mode, access, share, buffer size, and options.</summary>
    Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options);

    /// <summary>Opens a file stream using <see cref="FileStreamOptions"/>.</summary>
    Stream New(ReadOnlySpan<char> path, FileStreamOptions options);

    /// <summary>Wraps an existing <see cref="FileStream"/>.</summary>
    Stream Wrap(FileStream fileStream);
}
