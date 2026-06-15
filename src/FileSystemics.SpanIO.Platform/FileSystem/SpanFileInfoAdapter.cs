using FileSystemics.Abstractions;

namespace FileSystemics.IO;

internal sealed class SpanFileInfoAdapter : ISpanFileInfo {
    private readonly ISpanFileSystem _fileSystem;
    private OwnedSpanPath _path;

    internal SpanFileInfoAdapter(ISpanFileSystem fileSystem, ReadOnlySpan<char> fullName) {
        _fileSystem = fileSystem;
        _path = new OwnedSpanPath(fullName);
    }

    internal SpanFileInfoAdapter(ISpanFileSystem fileSystem, string fullName) {
        _fileSystem = fileSystem;
        _path = new OwnedSpanPath(fullName);
    }

    public ReadOnlySpan<char> FullName => _path.Span;

    public ReadOnlySpan<char> Name => SpanInfoSource.File(_path.Span).Name;

    public ReadOnlySpan<char> Extension => SpanInfoSource.File(_path.Span).Extension;

    public ReadOnlySpan<char> DirectoryName => SpanInfoSource.File(_path.Span).DirectoryName;

    public ISpanDirectoryInfo Directory =>
        _fileSystem.DirectoryInfo.New(SpanInfoSource.File(_path.Span).DirectoryName);

    public bool Exists => SpanInfoSource.File(_path.Span).Exists;

    public long Length => SpanInfoSource.File(_path.Span).Length;

    public bool IsReadOnly => SpanInfoSource.File(_path.Span).IsReadOnly;

    public FileAttributes Attributes {
        get => SpanInfoSource.File(_path.Span).Attributes;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.Attributes = value;
        }
    }

    public DateTime CreationTime {
        get => SpanInfoSource.File(_path.Span).CreationTime;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.CreationTime = value;
        }
    }

    public DateTime CreationTimeUtc {
        get => SpanInfoSource.File(_path.Span).CreationTimeUtc;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.CreationTimeUtc = value;
        }
    }

    public DateTime LastAccessTime {
        get => SpanInfoSource.File(_path.Span).LastAccessTime;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.LastAccessTime = value;
        }
    }

    public DateTime LastAccessTimeUtc {
        get => SpanInfoSource.File(_path.Span).LastAccessTimeUtc;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.LastAccessTimeUtc = value;
        }
    }

    public DateTime LastWriteTime {
        get => SpanInfoSource.File(_path.Span).LastWriteTime;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.LastWriteTime = value;
        }
    }

    public DateTime LastWriteTimeUtc {
        get => SpanInfoSource.File(_path.Span).LastWriteTimeUtc;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.LastWriteTimeUtc = value;
        }
    }

    public UnixFileMode UnixFileMode {
        get => SpanInfoSource.File(_path.Span).UnixFileMode;
        set {
            SpanFileInfo info = SpanInfoSource.File(_path.Span);
            info.UnixFileMode = value;
        }
    }

    public void Refresh() => SpanInfoSource.File(_path.Span).Refresh();

    public bool TryGetLinkTarget(Span<char> destination, out int charsWritten) =>
        SpanInfoSource.File(_path.Span).TryGetLinkTarget(destination, out charsWritten);

    public void Delete() => SpanInfoSource.File(_path.Span).Delete();

    public void CopyTo(ReadOnlySpan<char> destFileName, bool overwrite = false) =>
        SpanInfoSource.File(_path.Span).CopyTo(destFileName, overwrite);

    public void CopyTo(ISpanFileInfo destination, bool overwrite = false) =>
        SpanInfoSource.File(_path.Span).CopyTo(destination.FullName, overwrite);

    public void MoveTo(ReadOnlySpan<char> destFileName) {
        SpanInfoSource.File(_path.Span).MoveTo(destFileName);
        _path.Replace(destFileName);
    }

    public void MoveTo(ISpanFileInfo destination) {
        SpanInfoSource.File(_path.Span).MoveTo(destination.FullName);
        _path.Replace(destination.FullName);
    }

    public Stream Create() => SpanInfoSource.File(_path.Span).Create();

    public Stream OpenRead() => SpanInfoSource.File(_path.Span).OpenRead();

    public Stream OpenWrite() => SpanInfoSource.File(_path.Span).OpenWrite();

    public Stream Open(FileMode mode) => SpanInfoSource.File(_path.Span).Open(mode);

    public Stream Open(FileMode mode, FileAccess access) => SpanInfoSource.File(_path.Span).Open(mode, access);

    public Stream Open(FileMode mode, FileAccess access, FileShare share) =>
        SpanInfoSource.File(_path.Span).Open(mode, access, share);

    public StreamReader OpenText() => SpanInfoSource.File(_path.Span).OpenText();

    public StreamWriter CreateText() => SpanInfoSource.File(_path.Span).CreateText();

    public StreamWriter AppendText() => SpanInfoSource.File(_path.Span).AppendText();

    public void Replace(ReadOnlySpan<char> destinationFileName, ReadOnlySpan<char> destinationBackupFileName, bool ignoreMetadataErrors = false) =>
        SpanInfoSource.File(_path.Span).Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors);

    public void Replace(ISpanFileInfo destinationFileName, ISpanFileInfo destinationBackupFileName, bool ignoreMetadataErrors = false) =>
        SpanInfoSource.File(_path.Span).Replace(destinationFileName.FullName, destinationBackupFileName.FullName, ignoreMetadataErrors);

    public bool TryReplace(ReadOnlySpan<char> destinationFileName, ReadOnlySpan<char> destinationBackupFileName, bool ignoreMetadataErrors = false) =>
        SpanInfoSource.File(_path.Span).TryReplace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors);

    public void Encrypt() => SpanInfoSource.File(_path.Span).Encrypt();

    public void Decrypt() => SpanInfoSource.File(_path.Span).Decrypt();
}
