using System.Text;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.Abstractions;

/// <summary>
/// Span-based file APIs mirroring common <c>System.IO.File</c> operations.
/// </summary>
public interface ISpanFile {
    /// <summary>Determines whether the specified file exists.</summary>
    bool Exists(ReadOnlySpan<char> path);

    /// <summary>Opens a file handle for the specified path.</summary>
    SafeFileHandle OpenHandle(
        ReadOnlySpan<char> path,
        FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        FileShare share = FileShare.Read,
        FileOptions options = FileOptions.None);

    /// <summary>Opens a file handle by combining a directory span and file name span.</summary>
    SafeFileHandle OpenHandle(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        FileShare share = FileShare.Read);

    /// <summary>Opens a file at the specified path.</summary>
    FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share);

    /// <summary>Opens a file at the specified path.</summary>
    FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access);

    /// <summary>Opens a file at the specified path.</summary>
    FileStream Open(ReadOnlySpan<char> path, FileMode mode);

    /// <summary>Opens an existing file for reading.</summary>
    FileStream OpenRead(ReadOnlySpan<char> path);

    /// <summary>Opens an existing file or creates a new file for writing.</summary>
    FileStream OpenWrite(ReadOnlySpan<char> path);

    /// <summary>Creates or overwrites a file.</summary>
    FileStream Create(ReadOnlySpan<char> path);

    /// <summary>Opens an existing file for reading text.</summary>
    StreamReader OpenText(ReadOnlySpan<char> path);

    /// <summary>Creates or opens a file for writing text.</summary>
    StreamWriter CreateText(ReadOnlySpan<char> path);

    /// <summary>Creates a text writer that appends to a file.</summary>
    StreamWriter AppendText(ReadOnlySpan<char> path);

    /// <summary>Deletes the specified file.</summary>
    void Delete(ReadOnlySpan<char> path);

    /// <summary>Copies an existing file.</summary>
    void Copy(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName, bool overwrite = false);

    /// <summary>Moves a file to a new location.</summary>
    void Move(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName);

    /// <summary>Reads all bytes from a file.</summary>
    byte[] ReadAllBytes(ReadOnlySpan<char> path);

    /// <summary>Writes bytes to a file.</summary>
    void WriteAllBytes(ReadOnlySpan<char> path, ReadOnlySpan<byte> bytes);

    /// <summary>Reads all text from a file.</summary>
    string ReadAllText(ReadOnlySpan<char> path, Encoding? encoding = null);

    /// <summary>Writes text to a file.</summary>
    void WriteAllText(ReadOnlySpan<char> path, ReadOnlySpan<char> contents, Encoding? encoding = null);

    /// <summary>Gets the attributes of a file.</summary>
    FileAttributes GetAttributes(ReadOnlySpan<char> path);

    /// <summary>Sets the attributes of a file.</summary>
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

    /// <summary>Replaces the contents of a file with the contents of another file.</summary>
    void Replace(
        ReadOnlySpan<char> sourceFileName,
        ReadOnlySpan<char> destinationFileName,
        ReadOnlySpan<char> destinationBackupFileName,
        bool ignoreMetadataErrors = false);

    /// <summary>Encrypts a file.</summary>
    void Encrypt(ReadOnlySpan<char> path);

    /// <summary>Decrypts a file.</summary>
    void Decrypt(ReadOnlySpan<char> path);
}
