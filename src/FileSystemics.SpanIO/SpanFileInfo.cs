using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

/// <summary>
/// Span-based file metadata and operations mirroring <see cref="FileInfo"/>.
/// </summary>
public readonly ref struct SpanFileInfo {
    private readonly ReadOnlySpan<char> _fullPath;

    /// <summary>Initializes file metadata for the specified path.</summary>
    public SpanFileInfo(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));
        _fullPath = path;
    }

    /// <summary>Gets the fully qualified path.</summary>
    public ReadOnlySpan<char> FullName => _fullPath;

    /// <summary>Gets the file name and extension.</summary>
    public ReadOnlySpan<char> Name => SpanPath.GetFileName(_fullPath);

    /// <summary>Gets the file extension.</summary>
    public ReadOnlySpan<char> Extension => SpanPath.GetExtension(_fullPath);

    /// <summary>Gets the directory name portion of the path.</summary>
    public ReadOnlySpan<char> DirectoryName => SpanPath.GetDirectoryName(_fullPath);

    /// <summary>Gets the parent directory.</summary>
    public SpanDirectoryInfo Directory {
        get {
            ReadOnlySpan<char> directory = SpanPath.GetDirectoryName(_fullPath);
            if (directory.IsEmpty) {
                throw new IOException("The directory name is invalid.");
            }

            return new SpanDirectoryInfo(directory);
        }
    }

    /// <summary>Gets whether the file exists.</summary>
    public bool Exists => SpanFile.Exists(_fullPath);

    /// <summary>Gets the file size in bytes.</summary>
    public long Length => PlatformOps.GetLength(_fullPath);

    /// <summary>Gets whether the file is read-only.</summary>
    public bool IsReadOnly => (Attributes & FileAttributes.ReadOnly) != 0;

    /// <summary>Gets or sets file attributes.</summary>
    public FileAttributes Attributes {
        get => SpanFile.GetAttributes(_fullPath);
        set => SpanFile.SetAttributes(_fullPath, value);
    }

    /// <summary>Gets or sets local creation time.</summary>
    public DateTime CreationTime {
        get => SpanFile.GetCreationTime(_fullPath);
        set => SpanFile.SetCreationTime(_fullPath, value);
    }

    /// <summary>Gets or sets UTC creation time.</summary>
    public DateTime CreationTimeUtc {
        get => SpanFile.GetCreationTimeUtc(_fullPath);
        set => SpanFile.SetCreationTimeUtc(_fullPath, value);
    }

    /// <summary>Gets or sets local last access time.</summary>
    public DateTime LastAccessTime {
        get => SpanFile.GetLastAccessTime(_fullPath);
        set => SpanFile.SetLastAccessTime(_fullPath, value);
    }

    /// <summary>Gets or sets UTC last access time.</summary>
    public DateTime LastAccessTimeUtc {
        get => SpanFile.GetLastAccessTimeUtc(_fullPath);
        set => SpanFile.SetLastAccessTimeUtc(_fullPath, value);
    }

    /// <summary>Gets or sets local last write time.</summary>
    public DateTime LastWriteTime {
        get => SpanFile.GetLastWriteTime(_fullPath);
        set => SpanFile.SetLastWriteTime(_fullPath, value);
    }

    /// <summary>Gets or sets UTC last write time.</summary>
    public DateTime LastWriteTimeUtc {
        get => SpanFile.GetLastWriteTimeUtc(_fullPath);
        set => SpanFile.SetLastWriteTimeUtc(_fullPath, value);
    }

    /// <summary>Gets or sets Unix file mode flags.</summary>
    public UnixFileMode UnixFileMode {
        get => SpanFile.GetUnixFileMode(_fullPath);
        set => SpanFile.SetUnixFileMode(_fullPath, value);
    }

    /// <summary>Refreshes cached metadata from disk.</summary>
    public void Refresh() { }

    /// <summary>Gets the link target into a destination buffer.</summary>
    public bool TryGetLinkTarget(Span<char> destination, out int charsWritten) =>
        PlatformOps.TryGetLinkTarget(_fullPath, destination, out charsWritten);

    /// <summary>Permanently deletes the file.</summary>
    public void Delete() => SpanFile.Delete(_fullPath);

    /// <summary>Copies the file to a destination path.</summary>
    public void CopyTo(ReadOnlySpan<char> destFileName, bool overwrite = false) {
        PathArgumentValidation.ValidatePath(destFileName, nameof(destFileName));
        SpanFile.Copy(_fullPath, destFileName, overwrite);
    }

    /// <summary>Copies the file to another file info instance.</summary>
    public void CopyTo(SpanFileInfo destination, bool overwrite = false) =>
        SpanFile.Copy(_fullPath, destination._fullPath, overwrite);

    /// <summary>Moves the file to a destination path.</summary>
    public void MoveTo(ReadOnlySpan<char> destFileName) {
        PathArgumentValidation.ValidatePath(destFileName, nameof(destFileName));
        SpanFile.Move(_fullPath, destFileName);
    }

    /// <summary>Moves the file to another file info instance.</summary>
    public void MoveTo(SpanFileInfo destination) =>
        SpanFile.Move(_fullPath, destination._fullPath);

    /// <summary>Creates or overwrites the file.</summary>
    public FileStream Create() => SpanFile.Create(_fullPath);

    /// <summary>Opens the file for reading.</summary>
    public FileStream OpenRead() => SpanFile.OpenRead(_fullPath);

    /// <summary>Opens the file for writing.</summary>
    public FileStream OpenWrite() => SpanFile.OpenWrite(_fullPath);

    /// <summary>Opens the file with the specified mode.</summary>
    public FileStream Open(FileMode mode) => SpanFile.Open(_fullPath, mode);

    /// <summary>Opens the file with the specified mode and access.</summary>
    public FileStream Open(FileMode mode, FileAccess access) => SpanFile.Open(_fullPath, mode, access);

    /// <summary>Opens the file with the specified mode, access, and share.</summary>
    public FileStream Open(FileMode mode, FileAccess access, FileShare share) =>
        SpanFile.Open(_fullPath, mode, access, share);

    /// <summary>Opens the file for reading text.</summary>
    public StreamReader OpenText() => SpanFile.OpenText(_fullPath);

    /// <summary>Creates a text writer that overwrites the file.</summary>
    public StreamWriter CreateText() => SpanFile.CreateText(_fullPath);

    /// <summary>Creates a text writer that appends to the file.</summary>
    public StreamWriter AppendText() => SpanFile.AppendText(_fullPath);

    /// <summary>Replaces the file contents with another file.</summary>
    public void Replace(ReadOnlySpan<char> destinationFileName, ReadOnlySpan<char> destinationBackupFileName, bool ignoreMetadataErrors = false) =>
        SpanFile.Replace(destinationFileName, _fullPath, destinationBackupFileName, ignoreMetadataErrors);

    /// <summary>Replaces the file contents with another file info instance.</summary>
    public void Replace(SpanFileInfo destinationFileName, SpanFileInfo destinationBackupFileName, bool ignoreMetadataErrors = false) =>
        Replace(destinationFileName._fullPath, destinationBackupFileName._fullPath, ignoreMetadataErrors);

    /// <summary>Attempts to replace the file contents with another file.</summary>
    public bool TryReplace(ReadOnlySpan<char> destinationFileName, ReadOnlySpan<char> destinationBackupFileName, bool ignoreMetadataErrors = false) {
        SpanFile.Replace(destinationFileName, _fullPath, destinationBackupFileName, ignoreMetadataErrors);
        return true;
    }

    /// <summary>Encrypts the file.</summary>
    public void Encrypt() => SpanFile.Encrypt(_fullPath);

    /// <summary>Decrypts the file.</summary>
    public void Decrypt() => SpanFile.Decrypt(_fullPath);
}
