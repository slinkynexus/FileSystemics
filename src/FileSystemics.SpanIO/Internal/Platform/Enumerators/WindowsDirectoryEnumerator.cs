using FileSystemics.IO.Interop;

namespace FileSystemics.IO.Internal;

internal sealed class WindowsDirectoryEnumerator : IFileSystemDirectoryEnumerator {
    private readonly DirectoryEntryKind _kind;
    private nint _handle;
    private InteropWindowsFind.Win32FindData _currentEntry;
    private bool _hasCurrentEntry;
    private bool _findFirstPending = true;
    private bool _disposed;

    internal WindowsDirectoryEnumerator(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind) {
        _kind = kind;
        _handle = InteropConstants.INVALID_HANDLE_VALUE;

        ReadOnlySpan<char> pattern = searchPattern.IsEmpty ? "*" : searchPattern;
        Span<char> joined = stackalloc char[SpanPath.GetJoinLength(directory, pattern) + 1];
        SpanPath.TryJoin(directory, pattern, joined, out int joinedLength);
        joined = joined[..joinedLength];

        int searchCapacity = WindowsNativePathEncoding.GetCapacity(joined);
        Span<char> searchPath = searchCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS
            ? stackalloc char[searchCapacity]
            : new char[searchCapacity];
        WindowsNativePathEncoding.Encode(joined, searchPath, out int searchPathLength);
        searchPath[searchPathLength] = '\0';

        InteropWindowsFind.Win32FindData data = default;
        _handle = InteropWindowsFind.FindFirstFileW(searchPath, ref data);
        if (_handle == InteropConstants.INVALID_HANDLE_VALUE) {
            InteropHelpers.ThrowExceptionForLastError();
        }

        _hasCurrentEntry = AcceptEntry(data);
    }

    public bool IsDirectory { get; private set; }

    public bool MoveNext() {
        if (_disposed) {
            return false;
        }

        if (_findFirstPending) {
            _findFirstPending = false;
            if (_hasCurrentEntry) {
                return true;
            }
        }

        while (true) {
            InteropWindowsFind.Win32FindData data = default;
            if (!InteropWindowsFind.FindNextFileW(_handle, ref data)) {
                _hasCurrentEntry = false;
                return false;
            }

            if (AcceptEntry(data)) {
                return true;
            }
        }
    }

    public bool TryGetCurrentEntryName(Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        if (!_hasCurrentEntry) {
            return false;
        }

        ReadOnlySpan<char> name = _currentEntry.cFileName.AsSpan().TrimEnd('\0');
        if (name.Length > destination.Length) {
            return false;
        }

        name.CopyTo(destination);
        charsWritten = name.Length;
        return true;
    }

    public void Dispose() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        _hasCurrentEntry = false;
        if (_handle != InteropConstants.INVALID_HANDLE_VALUE) {
            InteropWindowsFind.FindClose(_handle);
            _handle = InteropConstants.INVALID_HANDLE_VALUE;
        }
    }

    private bool AcceptEntry(InteropWindowsFind.Win32FindData data) {
        ReadOnlySpan<char> name = data.cFileName.AsSpan().TrimEnd('\0');
        if (name.SequenceEqual(".") || name.SequenceEqual("..")) {
            return false;
        }

        IsDirectory = (data.dwFileAttributes & InteropConstants.FILE_ATTRIBUTE_DIRECTORY) != 0;
        if (_kind == DirectoryEntryKind.Files && IsDirectory) {
            return false;
        }

        if (_kind == DirectoryEntryKind.Directories && !IsDirectory) {
            return false;
        }

        _currentEntry = data;
        _hasCurrentEntry = true;
        return true;
    }
}
