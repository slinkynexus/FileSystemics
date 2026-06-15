using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

/// <summary>
/// Span-based directory metadata and operations mirroring <see cref="DirectoryInfo"/>.
/// </summary>
public readonly ref struct SpanDirectoryInfo {
    private readonly ReadOnlySpan<char> _fullPath;

    /// <summary>Initializes directory metadata for the specified path.</summary>
    public SpanDirectoryInfo(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));
        _fullPath = path;
    }

    /// <summary>Gets the fully qualified path.</summary>
    public ReadOnlySpan<char> FullName => _fullPath;

    /// <summary>Gets the directory name.</summary>
    public ReadOnlySpan<char> Name => SpanPath.GetFileName(_fullPath);

    /// <summary>Gets whether the directory exists.</summary>
    public bool Exists => SpanDirectory.Exists(_fullPath);

    /// <summary>Gets or sets file attributes.</summary>
    public FileAttributes Attributes {
        get => SpanDirectory.GetAttributes(_fullPath);
        set => SpanDirectory.SetAttributes(_fullPath, value);
    }

    /// <summary>Gets or sets local creation time.</summary>
    public DateTime CreationTime {
        get => SpanDirectory.GetCreationTime(_fullPath);
        set => SpanDirectory.SetCreationTime(_fullPath, value);
    }

    /// <summary>Gets or sets UTC creation time.</summary>
    public DateTime CreationTimeUtc {
        get => SpanDirectory.GetCreationTimeUtc(_fullPath);
        set => SpanDirectory.SetCreationTimeUtc(_fullPath, value);
    }

    /// <summary>Gets or sets local last access time.</summary>
    public DateTime LastAccessTime {
        get => SpanDirectory.GetLastAccessTime(_fullPath);
        set => SpanDirectory.SetLastAccessTime(_fullPath, value);
    }

    /// <summary>Gets or sets UTC last access time.</summary>
    public DateTime LastAccessTimeUtc {
        get => SpanDirectory.GetLastAccessTimeUtc(_fullPath);
        set => SpanDirectory.SetLastAccessTimeUtc(_fullPath, value);
    }

    /// <summary>Gets or sets local last write time.</summary>
    public DateTime LastWriteTime {
        get => SpanDirectory.GetLastWriteTime(_fullPath);
        set => SpanDirectory.SetLastWriteTime(_fullPath, value);
    }

    /// <summary>Gets or sets UTC last write time.</summary>
    public DateTime LastWriteTimeUtc {
        get => SpanDirectory.GetLastWriteTimeUtc(_fullPath);
        set => SpanDirectory.SetLastWriteTimeUtc(_fullPath, value);
    }

    /// <summary>Gets or sets Unix file mode flags.</summary>
    public UnixFileMode UnixFileMode {
        get => SpanDirectory.GetUnixFileMode(_fullPath);
        set => SpanDirectory.SetUnixFileMode(_fullPath, value);
    }

    /// <summary>Gets the parent directory.</summary>
    public SpanDirectoryInfo Parent {
        get {
            ReadOnlySpan<char> parent = SpanPath.GetDirectoryName(_fullPath);
            if (parent.IsEmpty) {
                throw new IOException("Cannot retrieve parent directory.");
            }

            return new SpanDirectoryInfo(parent);
        }
    }

    /// <summary>Gets the root directory.</summary>
    public SpanDirectoryInfo Root {
        get {
            ReadOnlySpan<char> root = SpanPath.GetPathRoot(_fullPath);
            if (root.IsEmpty) {
                throw new IOException("The root directory is invalid.");
            }

            return new SpanDirectoryInfo(root);
        }
    }

    /// <summary>Refreshes cached metadata from disk.</summary>
    public void Refresh() { }

    /// <summary>Gets the link target into a destination buffer.</summary>
    public bool TryGetLinkTarget(Span<char> destination, out int charsWritten) =>
        PlatformOps.TryGetLinkTarget(_fullPath, destination, out charsWritten);

    /// <summary>Creates the directory if it does not exist.</summary>
    public void Create() => SpanDirectory.CreateDirectory(_fullPath);

    /// <summary>Creates a subdirectory into a destination buffer.</summary>
    public bool TryCreateSubdirectory(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        if (PathArgumentValidation.IsEffectivelyEmpty(path)) {
            throw SpanIOException.EmptyPath(nameof(path));
        }

        if (SpanPath.IsPathRooted(path)) {
            throw SpanIOException.RootedSubPath(nameof(path));
        }

        ReadOnlySpan<char> subpath = path;
        if (SpanPath.AltDirectorySeparatorChar != SpanPath.DirectorySeparatorChar) {
            bool needsNormalization = false;
            for (int i = 0; i < path.Length; i++) {
                if (path[i] == SpanPath.AltDirectorySeparatorChar) {
                    needsNormalization = true;
                    break;
                }
            }

            if (needsNormalization) {
                char[] normalized = new char[path.Length];
                for (int i = 0; i < path.Length; i++) {
                    char ch = path[i];
                    normalized[i] = ch == SpanPath.AltDirectorySeparatorChar
                        ? SpanPath.DirectorySeparatorChar
                        : ch;
                }

                subpath = normalized;
            }
        }

        if (!SpanPath.TryJoin(_fullPath, subpath, destination, out charsWritten)) {
            return false;
        }

        ReadOnlySpan<char> combinedPath = destination[..charsWritten];
        if (!IsNestedSubdirectoryPath(combinedPath)) {
            throw SpanIOException.InvalidSubPath(path, _fullPath, nameof(path));
        }

        SpanDirectory.CreateDirectory(combinedPath);
        return true;
    }

    /// <summary>Creates a subdirectory and returns its metadata.</summary>
    public SpanDirectoryInfo CreateSubdirectory(ReadOnlySpan<char> path, Span<char> destination) {
        if (!TryCreateSubdirectory(path, destination, out int charsWritten)) {
            throw SpanIOException.DestinationTooSmall();
        }

        return new SpanDirectoryInfo(destination[..charsWritten]);
    }

    /// <summary>Deletes the directory when empty.</summary>
    public void Delete() => Delete(recursive: false);

    /// <summary>Deletes the directory, optionally recursively.</summary>
    public void Delete(bool recursive) => SpanDirectory.Delete(_fullPath, recursive);

    /// <summary>Moves the directory into a destination buffer.</summary>
    public void MoveTo(ReadOnlySpan<char> destDirName, Span<char> destination) {
        if (!TryMoveTo(destDirName, destination, out _)) {
            throw SpanIOException.DestinationTooSmall();
        }
    }

    /// <summary>Moves the directory, writing the new path into a destination buffer.</summary>
    public bool TryMoveTo(ReadOnlySpan<char> destDirName, Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        PathArgumentValidation.ValidatePath(destDirName, nameof(destDirName));

        ReadOnlySpan<char> name = SpanPath.GetFileName(_fullPath);
        if (!SpanPath.TryJoin(destDirName, name, destination, out charsWritten)) {
            return false;
        }

        SpanDirectory.Move(_fullPath, destination[..charsWritten]);

        return true;
    }

    /// <summary>Enumerates files into <paramref name="buffer"/>.</summary>
    public SpanDirectoryEntryEnumerator EnumerateFiles(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        new(_fullPath, searchPattern, DirectoryEntryKind.Files, buffer);

    /// <summary>Enumerates subdirectories into <paramref name="buffer"/>.</summary>
    public SpanDirectoryEntryEnumerator EnumerateDirectories(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        new(_fullPath, searchPattern, DirectoryEntryKind.Directories, buffer);

    /// <summary>Enumerates files and subdirectories into <paramref name="buffer"/>.</summary>
    public SpanDirectoryEntryEnumerator EnumerateFileSystemEntries(
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        new(_fullPath, searchPattern, DirectoryEntryKind.All, buffer);

    private bool IsNestedSubdirectoryPath(ReadOnlySpan<char> combinedPath) {
        ReadOnlySpan<char> trimmedNew = SpanPath.TrimEndingDirectorySeparator(combinedPath);
        ReadOnlySpan<char> trimmedCurrent = SpanPath.TrimEndingDirectorySeparator(_fullPath);
        StringComparison comparison = NativePlatformTable.PathComparison;

        if (!trimmedNew.StartsWith(trimmedCurrent, comparison)) {
            return false;
        }

        if (trimmedNew.Length == trimmedCurrent.Length) {
            return true;
        }

        if (trimmedNew.Length <= trimmedCurrent.Length) {
            return false;
        }

        char separator = combinedPath[trimmedCurrent.Length];
        return separator == SpanPath.DirectorySeparatorChar || separator == SpanPath.AltDirectorySeparatorChar;
    }
}

/// <summary>
/// Enumerates directory entries one at a time into a caller-provided buffer.
/// </summary>
/// <remarks>
/// The buffer passed to <see cref="SpanDirectory.EnumerateFiles(ReadOnlySpan{char}, Span{char}, ReadOnlySpan{char})"/> must be at least as large as the longest expected full path.
/// Directory and search-pattern spans must remain valid until this enumerator is disposed.
/// Each <see cref="Current"/> path is valid only until the next <c>MoveNext</c> or <c>foreach</c> iteration.
/// Enumeration stops when a path does not fit.
/// </remarks>
public ref struct SpanDirectoryEntryEnumerator {
    private IFileSystemDirectoryEnumerator? _enumerator;
    private ReadOnlySpan<char> _directory;
    private ReadOnlySpan<char> _searchPattern;
    private Span<char> _buffer;
    private int _currentLength;

    internal SpanDirectoryEntryEnumerator(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind,
        Span<char> buffer) {
        _directory = directory;
        _searchPattern = searchPattern;
        _enumerator = PlatformOps.EnumerateDirectory(directory, searchPattern, kind);
        _buffer = buffer;
        _currentLength = 0;
    }

    /// <summary>Gets whether the current entry is a directory.</summary>
    public bool IsDirectory => _enumerator?.IsDirectory ?? false;

    /// <summary>Gets the current entry path.</summary>
    public ReadOnlySpan<char> Current => _buffer[.._currentLength];

    /// <summary>Enables <c>foreach</c> over this enumerator.</summary>
    public readonly SpanDirectoryEntryEnumerator GetEnumerator() => this;

    /// <summary>Advances to the next entry.</summary>
    public bool MoveNext() {
        if (_enumerator is null) {
            return false;
        }

        Span<char> nameBuffer = stackalloc char[DirectoryEntryPathReader.MaxEntryNameChars];
        while (_enumerator.MoveNext()) {
            if (!_enumerator.TryGetCurrentEntryName(nameBuffer, out int nameLength)) {
                return false;
            }

            ReadOnlySpan<char> name = nameBuffer[..nameLength];
            if (!PathPatternMatcher.Matches(name, _searchPattern)) {
                continue;
            }

            if (SpanPath.TryJoin(_directory, name, _buffer, out _currentLength)) {
                return true;
            }

            return false;
        }

        _currentLength = 0;
        return false;
    }

    /// <summary>Releases enumeration resources.</summary>
    public void Dispose() => _enumerator?.Dispose();
}
