namespace FileSystemics.Abstractions;

/// <summary>
/// Directory metadata and operations mirroring <see cref="DirectoryInfo"/>.
/// </summary>
public interface ISpanDirectoryInfo {
    /// <summary>Gets the fully qualified path.</summary>
    ReadOnlySpan<char> FullName { get; }

    /// <summary>Gets the directory name.</summary>
    ReadOnlySpan<char> Name { get; }

    /// <summary>Gets whether the directory exists.</summary>
    bool Exists { get; }

    /// <summary>Gets or sets file attributes.</summary>
    FileAttributes Attributes { get; set; }

    /// <summary>Gets or sets local creation time.</summary>
    DateTime CreationTime { get; set; }

    /// <summary>Gets or sets UTC creation time.</summary>
    DateTime CreationTimeUtc { get; set; }

    /// <summary>Gets or sets local last access time.</summary>
    DateTime LastAccessTime { get; set; }

    /// <summary>Gets or sets UTC last access time.</summary>
    DateTime LastAccessTimeUtc { get; set; }

    /// <summary>Gets or sets local last write time.</summary>
    DateTime LastWriteTime { get; set; }

    /// <summary>Gets or sets UTC last write time.</summary>
    DateTime LastWriteTimeUtc { get; set; }

    /// <summary>Gets or sets Unix file mode flags.</summary>
    UnixFileMode UnixFileMode { get; set; }

    /// <summary>Gets the parent directory.</summary>
    ISpanDirectoryInfo Parent { get; }

    /// <summary>Gets the root directory.</summary>
    ISpanDirectoryInfo Root { get; }

    /// <summary>Refreshes cached metadata from disk.</summary>
    void Refresh();

    /// <summary>Gets the link target into a destination buffer.</summary>
    bool TryGetLinkTarget(Span<char> destination, out int charsWritten);

    /// <summary>Creates the directory if it does not exist.</summary>
    void Create();

    /// <summary>Creates a subdirectory into a destination buffer.</summary>
    bool TryCreateSubdirectory(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten);

    /// <summary>Creates a subdirectory and returns its metadata.</summary>
    ISpanDirectoryInfo CreateSubdirectory(ReadOnlySpan<char> path);

    /// <summary>Deletes the directory when empty.</summary>
    void Delete();

    /// <summary>Deletes the directory, optionally recursively.</summary>
    void Delete(bool recursive);

    /// <summary>Moves the directory into a destination buffer.</summary>
    bool TryMoveTo(ReadOnlySpan<char> destDirName, Span<char> destination, out int charsWritten);

    /// <summary>Moves the directory to a new parent path.</summary>
    void MoveTo(ReadOnlySpan<char> destDirName);

    /// <summary>Enumerates files into <paramref name="buffer"/>.</summary>
    FileSystemics.IO.SpanDirectoryEntryEnumerator EnumerateFiles(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default);

    /// <summary>Enumerates subdirectories into <paramref name="buffer"/>.</summary>
    FileSystemics.IO.SpanDirectoryEntryEnumerator EnumerateDirectories(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default);

    /// <summary>Enumerates files and subdirectories into <paramref name="buffer"/>.</summary>
    FileSystemics.IO.SpanDirectoryEntryEnumerator EnumerateFileSystemEntries(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns file names using a caller-provided enumeration buffer.</summary>
    string[] GetFiles(Span<char> buffer, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns file names in the directory.</summary>
    string[] GetFiles(ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns subdirectory names using a caller-provided enumeration buffer.</summary>
    string[] GetDirectories(Span<char> buffer, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns subdirectory names in the directory.</summary>
    string[] GetDirectories(ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns file and subdirectory names using a caller-provided enumeration buffer.</summary>
    string[] GetFileSystemEntries(Span<char> buffer, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns file and subdirectory names in the directory.</summary>
    string[] GetFileSystemEntries(ReadOnlySpan<char> searchPattern = default);
}
