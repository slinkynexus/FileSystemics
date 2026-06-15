namespace FileSystemics.Abstractions;

/// <summary>
/// File metadata and operations mirroring <see cref="FileInfo"/>.
/// </summary>
public interface ISpanFileInfo {
    /// <summary>Gets the fully qualified path.</summary>
    ReadOnlySpan<char> FullName { get; }

    /// <summary>Gets the file name and extension.</summary>
    ReadOnlySpan<char> Name { get; }

    /// <summary>Gets the file extension.</summary>
    ReadOnlySpan<char> Extension { get; }

    /// <summary>Gets the directory name portion of the path.</summary>
    ReadOnlySpan<char> DirectoryName { get; }

    /// <summary>Gets the parent directory.</summary>
    ISpanDirectoryInfo Directory { get; }

    /// <summary>Gets whether the file exists.</summary>
    bool Exists { get; }

    /// <summary>Gets the file size in bytes.</summary>
    long Length { get; }

    /// <summary>Gets whether the file is read-only.</summary>
    bool IsReadOnly { get; }

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

    /// <summary>Refreshes cached metadata from disk.</summary>
    void Refresh();

    /// <summary>Gets the link target into a destination buffer.</summary>
    bool TryGetLinkTarget(Span<char> destination, out int charsWritten);

    /// <summary>Permanently deletes the file.</summary>
    void Delete();

    /// <summary>Copies the file to a destination path.</summary>
    void CopyTo(ReadOnlySpan<char> destFileName, bool overwrite = false);

    /// <summary>Copies the file to another file info instance.</summary>
    void CopyTo(ISpanFileInfo destination, bool overwrite = false);

    /// <summary>Moves the file to a destination path.</summary>
    void MoveTo(ReadOnlySpan<char> destFileName);

    /// <summary>Moves the file to another file info instance.</summary>
    void MoveTo(ISpanFileInfo destination);

    /// <summary>Creates or overwrites the file.</summary>
    Stream Create();

    /// <summary>Opens the file for reading.</summary>
    Stream OpenRead();

    /// <summary>Opens the file for writing.</summary>
    Stream OpenWrite();

    /// <summary>Opens the file with the specified mode.</summary>
    Stream Open(FileMode mode);

    /// <summary>Opens the file with the specified mode and access.</summary>
    Stream Open(FileMode mode, FileAccess access);

    /// <summary>Opens the file with the specified mode, access, and share.</summary>
    Stream Open(FileMode mode, FileAccess access, FileShare share);

    /// <summary>Opens the file for reading text.</summary>
    StreamReader OpenText();

    /// <summary>Creates a text writer that overwrites the file.</summary>
    StreamWriter CreateText();

    /// <summary>Creates a text writer that appends to the file.</summary>
    StreamWriter AppendText();

    /// <summary>Replaces the file contents with another file.</summary>
    void Replace(ReadOnlySpan<char> destinationFileName, ReadOnlySpan<char> destinationBackupFileName, bool ignoreMetadataErrors = false);

    /// <summary>Replaces the file contents with another file info instance.</summary>
    void Replace(ISpanFileInfo destinationFileName, ISpanFileInfo destinationBackupFileName, bool ignoreMetadataErrors = false);

    /// <summary>Attempts to replace the file contents with another file.</summary>
    bool TryReplace(ReadOnlySpan<char> destinationFileName, ReadOnlySpan<char> destinationBackupFileName, bool ignoreMetadataErrors = false);

    /// <summary>Encrypts the file.</summary>
    void Encrypt();

    /// <summary>Decrypts the file.</summary>
    void Decrypt();
}
