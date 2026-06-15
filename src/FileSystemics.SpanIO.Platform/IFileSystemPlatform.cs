using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO;

/// <summary>
/// File-system operations for a platform host.
/// </summary>
public interface IFileSystemPlatform {
    /// <summary>Determines whether a file exists.</summary>
    bool Exists(ReadOnlySpan<char> path);

    /// <summary>Determines whether a directory exists.</summary>
    bool DirectoryExists(ReadOnlySpan<char> path);

    /// <summary>Opens a file handle for the specified path.</summary>
    SafeFileHandle OpenHandle(
        ReadOnlySpan<char> path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options);

    /// <summary>Opens a file handle by combining directory and file name spans.</summary>
    SafeFileHandle OpenHandle(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        FileMode mode,
        FileAccess access,
        FileShare share);

    /// <summary>Deletes a file.</summary>
    void Delete(ReadOnlySpan<char> path);

    /// <summary>Copies a file.</summary>
    void Copy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite);

    /// <summary>Moves a file or directory.</summary>
    void Move(ReadOnlySpan<char> source, ReadOnlySpan<char> destination);

    /// <summary>Gets file attributes.</summary>
    FileAttributes GetAttributes(ReadOnlySpan<char> path);

    /// <summary>Sets file attributes.</summary>
    void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes);

    /// <summary>Gets UTC creation time.</summary>
    DateTime GetCreationTimeUtc(ReadOnlySpan<char> path);

    /// <summary>Gets UTC last access time.</summary>
    DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path);

    /// <summary>Gets UTC last write time.</summary>
    DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path);

    /// <summary>Sets UTC creation time.</summary>
    void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value);

    /// <summary>Sets UTC last access time.</summary>
    void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value);

    /// <summary>Sets UTC last write time.</summary>
    void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value);

    /// <summary>Gets file size in bytes.</summary>
    long GetLength(ReadOnlySpan<char> path);

    /// <summary>Creates a directory.</summary>
    void CreateDirectory(ReadOnlySpan<char> path);

    /// <summary>Deletes a directory.</summary>
    void DeleteDirectory(ReadOnlySpan<char> path, bool recursive);

    /// <summary>Enumerates directory entries matching the search pattern.</summary>
    IFileSystemDirectoryEnumerator EnumerateDirectory(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind);

    /// <summary>Gets the link target into a destination buffer.</summary>
    bool TryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten);

    /// <summary>Gets Unix file mode flags.</summary>
    UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path);

    /// <summary>Sets Unix file mode flags.</summary>
    void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode);

    /// <summary>Encrypts a file.</summary>
    void Encrypt(ReadOnlySpan<char> path);

    /// <summary>Decrypts a file.</summary>
    void Decrypt(ReadOnlySpan<char> path);
}
