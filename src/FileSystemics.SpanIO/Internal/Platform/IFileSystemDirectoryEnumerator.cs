namespace FileSystemics.IO.Internal;

internal interface IFileSystemDirectoryEnumerator : IDisposable {
    bool IsDirectory { get; }

    bool MoveNext();

    bool TryGetCurrentEntryName(Span<char> destination, out int charsWritten);
}
