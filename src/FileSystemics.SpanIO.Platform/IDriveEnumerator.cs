namespace FileSystemics.IO;

/// <summary>
/// Enumerates logical drives for a platform host.
/// </summary>
public interface IDriveEnumerator : IDisposable {
    /// <summary>Advances to the next drive.</summary>
    bool MoveNext();

    /// <summary>Writes the current drive name into a destination buffer.</summary>
    bool TryGetCurrentDrive(Span<char> destination, out int charsWritten);
}
