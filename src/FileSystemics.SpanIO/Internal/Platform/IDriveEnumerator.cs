namespace FileSystemics.IO.Internal;

internal interface IDriveEnumerator : IDisposable {
    bool MoveNext();

    bool TryGetCurrentDrive(Span<char> destination, out int charsWritten);
}
