using FileSystemics.Abstractions;
using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

internal sealed class SpanDirectoryInfoAdapter : ISpanDirectoryInfo {
    private readonly ISpanFileSystem _fileSystem;
    private OwnedSpanPath _path;

    internal SpanDirectoryInfoAdapter(ISpanFileSystem fileSystem, ReadOnlySpan<char> fullName) {
        _fileSystem = fileSystem;
        _path = new OwnedSpanPath(fullName);
    }

    internal SpanDirectoryInfoAdapter(ISpanFileSystem fileSystem, string fullName) {
        _fileSystem = fileSystem;
        _path = new OwnedSpanPath(fullName);
    }

    public ReadOnlySpan<char> FullName => _path.Span;

    public ReadOnlySpan<char> Name => SpanInfoSource.Directory(_path.Span).Name;

    public bool Exists => SpanInfoSource.Directory(_path.Span).Exists;

    public FileAttributes Attributes {
        get => SpanInfoSource.Directory(_path.Span).Attributes;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.Attributes = value;
        }
    }

    public DateTime CreationTime {
        get => SpanInfoSource.Directory(_path.Span).CreationTime;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.CreationTime = value;
        }
    }

    public DateTime CreationTimeUtc {
        get => SpanInfoSource.Directory(_path.Span).CreationTimeUtc;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.CreationTimeUtc = value;
        }
    }

    public DateTime LastAccessTime {
        get => SpanInfoSource.Directory(_path.Span).LastAccessTime;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.LastAccessTime = value;
        }
    }

    public DateTime LastAccessTimeUtc {
        get => SpanInfoSource.Directory(_path.Span).LastAccessTimeUtc;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.LastAccessTimeUtc = value;
        }
    }

    public DateTime LastWriteTime {
        get => SpanInfoSource.Directory(_path.Span).LastWriteTime;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.LastWriteTime = value;
        }
    }

    public DateTime LastWriteTimeUtc {
        get => SpanInfoSource.Directory(_path.Span).LastWriteTimeUtc;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.LastWriteTimeUtc = value;
        }
    }

    public UnixFileMode UnixFileMode {
        get => SpanInfoSource.Directory(_path.Span).UnixFileMode;
        set {
            SpanDirectoryInfo info = SpanInfoSource.Directory(_path.Span);
            info.UnixFileMode = value;
        }
    }

    public ISpanDirectoryInfo Parent =>
        _fileSystem.DirectoryInfo.New(SpanInfoSource.Directory(_path.Span).Parent.FullName);

    public ISpanDirectoryInfo Root =>
        _fileSystem.DirectoryInfo.New(SpanInfoSource.Directory(_path.Span).Root.FullName);

    public void Refresh() => SpanInfoSource.Directory(_path.Span).Refresh();

    public bool TryGetLinkTarget(Span<char> destination, out int charsWritten) =>
        SpanInfoSource.Directory(_path.Span).TryGetLinkTarget(destination, out charsWritten);

    public void Create() => SpanInfoSource.Directory(_path.Span).Create();

    public bool TryCreateSubdirectory(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) =>
        SpanInfoSource.Directory(_path.Span).TryCreateSubdirectory(path, destination, out charsWritten);

    public ISpanDirectoryInfo CreateSubdirectory(ReadOnlySpan<char> path) {
        char[] destination = new char[_path.Span.Length + path.Length + 2];
        SpanDirectoryInfo subdirectory = SpanInfoSource.Directory(_path.Span).CreateSubdirectory(path, destination);
        return _fileSystem.DirectoryInfo.New(subdirectory.FullName);
    }

    public void Delete() => SpanInfoSource.Directory(_path.Span).Delete();

    public void Delete(bool recursive) => SpanInfoSource.Directory(_path.Span).Delete(recursive);

    public bool TryMoveTo(ReadOnlySpan<char> destDirName, Span<char> destination, out int charsWritten) {
        bool moved = SpanInfoSource.Directory(_path.Span).TryMoveTo(destDirName, destination, out charsWritten);
        if (moved) {
            _path.Replace(destination[..charsWritten]);
        }

        return moved;
    }

    public void MoveTo(ReadOnlySpan<char> destDirName) {
        char[] destination = new char[destDirName.Length + _path.Span.Length + 2];
        if (!TryMoveTo(destDirName, destination, out _)) {
            throw SpanIOException.DestinationTooSmall();
        }
    }

    public SpanDirectoryEntryEnumerator EnumerateFiles(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.EnumerateFiles(_path.Span, buffer, searchPattern);

    public SpanDirectoryEntryEnumerator EnumerateDirectories(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.EnumerateDirectories(_path.Span, buffer, searchPattern);

    public SpanDirectoryEntryEnumerator EnumerateFileSystemEntries(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.EnumerateFileSystemEntries(_path.Span, buffer, searchPattern);

    public string[] GetFiles(Span<char> buffer, ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.GetFiles(_path.Span, buffer, searchPattern);

    public string[] GetFiles(ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.GetFiles(_path.Span, searchPattern);

    public string[] GetDirectories(Span<char> buffer, ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.GetDirectories(_path.Span, buffer, searchPattern);

    public string[] GetDirectories(ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.GetDirectories(_path.Span, searchPattern);

    public string[] GetFileSystemEntries(Span<char> buffer, ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.GetFileSystemEntries(_path.Span, buffer, searchPattern);

    public string[] GetFileSystemEntries(ReadOnlySpan<char> searchPattern = default) =>
        SpanDirectory.GetFileSystemEntries(_path.Span, searchPattern);
}
