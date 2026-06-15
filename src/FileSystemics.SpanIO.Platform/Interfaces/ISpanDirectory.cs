namespace FileSystemics.Abstractions;

/// <summary>
/// Span-based directory APIs mirroring common <c>System.IO.Directory</c> operations.
/// </summary>
public interface ISpanDirectory {
    /// <summary>Determines whether the specified directory exists.</summary>
    bool Exists(ReadOnlySpan<char> path);

    /// <summary>Creates all directories in the specified path.</summary>
    void CreateDirectory(ReadOnlySpan<char> path);

    /// <summary>Deletes the specified directory.</summary>
    void Delete(ReadOnlySpan<char> path, bool recursive = false);

    /// <summary>Moves a directory to a new location.</summary>
    void Move(ReadOnlySpan<char> sourceDirName, ReadOnlySpan<char> destDirName);

    /// <summary>Gets the parent directory of the specified path.</summary>
    ReadOnlySpan<char> GetParent(ReadOnlySpan<char> path);

    /// <summary>Gets the root directory of the specified path.</summary>
    ReadOnlySpan<char> GetDirectoryRoot(ReadOnlySpan<char> path);

    /// <summary>Gets the current working directory.</summary>
    string GetCurrentDirectory();

    /// <summary>Sets the current working directory.</summary>
    void SetCurrentDirectory(ReadOnlySpan<char> path);

    /// <summary>Gets the names of logical drives on the computer.</summary>
    string[] GetLogicalDrives();

    /// <summary>Gets the attributes of the directory.</summary>
    FileAttributes GetAttributes(ReadOnlySpan<char> path);

    /// <summary>Sets the attributes of the directory.</summary>
    void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes);

    /// <summary>Gets local creation time.</summary>
    DateTime GetCreationTime(ReadOnlySpan<char> path);

    /// <summary>Gets UTC creation time.</summary>
    DateTime GetCreationTimeUtc(ReadOnlySpan<char> path);

    /// <summary>Sets local creation time.</summary>
    void SetCreationTime(ReadOnlySpan<char> path, DateTime creationTime);

    /// <summary>Sets UTC creation time.</summary>
    void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime creationTimeUtc);

    /// <summary>Gets local last access time.</summary>
    DateTime GetLastAccessTime(ReadOnlySpan<char> path);

    /// <summary>Gets UTC last access time.</summary>
    DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path);

    /// <summary>Sets local last access time.</summary>
    void SetLastAccessTime(ReadOnlySpan<char> path, DateTime lastAccessTime);

    /// <summary>Sets UTC last access time.</summary>
    void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime lastAccessTimeUtc);

    /// <summary>Gets local last write time.</summary>
    DateTime GetLastWriteTime(ReadOnlySpan<char> path);

    /// <summary>Gets UTC last write time.</summary>
    DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path);

    /// <summary>Sets local last write time.</summary>
    void SetLastWriteTime(ReadOnlySpan<char> path, DateTime lastWriteTime);

    /// <summary>Sets UTC last write time.</summary>
    void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime lastWriteTimeUtc);

    /// <summary>Gets Unix file mode flags.</summary>
    UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path);

    /// <summary>Sets Unix file mode flags.</summary>
    void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode);

    /// <summary>
    /// Returns a recommended <c>char</c> buffer capacity for enumerating entry paths under <paramref name="directoryPath"/>.
    /// </summary>
    int GetEntryPathBufferCapacity(ReadOnlySpan<char> directoryPath);

    /// <summary>Enumerates files into <paramref name="buffer"/>.</summary>
    FileSystemics.IO.SpanDirectoryEntryEnumerator EnumerateFiles(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default);

    /// <summary>Enumerates subdirectories into <paramref name="buffer"/>.</summary>
    FileSystemics.IO.SpanDirectoryEntryEnumerator EnumerateDirectories(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default);

    /// <summary>Enumerates files and subdirectories into <paramref name="buffer"/>.</summary>
    FileSystemics.IO.SpanDirectoryEntryEnumerator EnumerateFileSystemEntries(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns file names using a caller-provided enumeration buffer.</summary>
    string[] GetFiles(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns the names of files in the specified directory.</summary>
    string[] GetFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns subdirectory names using a caller-provided enumeration buffer.</summary>
    string[] GetDirectories(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns subdirectory names in the specified directory.</summary>
    string[] GetDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns file and subdirectory names using a caller-provided enumeration buffer.</summary>
    string[] GetFileSystemEntries(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Returns file and subdirectory names in the specified directory.</summary>
    string[] GetFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Enumerates file names in the specified directory.</summary>
    IEnumerable<string> EnumerateFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Enumerates subdirectory names in the specified directory.</summary>
    IEnumerable<string> EnumerateDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default);

    /// <summary>Enumerates file and subdirectory names in the specified directory.</summary>
    IEnumerable<string> EnumerateFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default);
}
