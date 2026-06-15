namespace FileSystemics.IO;

/// <summary>
/// Enumerates file-system entries under a directory.
/// </summary>
public interface IFileSystemDirectoryEnumerator : IDisposable {
    /// <summary>Gets whether the current entry is a directory.</summary>
    bool IsDirectory { get; }

    /// <summary>Advances to the next entry.</summary>
    bool MoveNext();

    /// <summary>Writes the current entry name into a destination buffer.</summary>
    bool TryGetCurrentEntryName(Span<char> destination, out int charsWritten);
}
