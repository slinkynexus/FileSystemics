using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.Internal;

internal interface IFileSystemPlatform<T> where T : IFileSystemPlatform<T> {
    static abstract bool Exists(ReadOnlySpan<char> path);

    static abstract bool DirectoryExists(ReadOnlySpan<char> path);

    static abstract SafeFileHandle OpenHandle(
        ReadOnlySpan<char> path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options);

    static abstract SafeFileHandle OpenHandle(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        FileMode mode,
        FileAccess access,
        FileShare share);

    static abstract void Delete(ReadOnlySpan<char> path);

    static abstract void Copy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite);

    static abstract void Move(ReadOnlySpan<char> source, ReadOnlySpan<char> destination);

    static abstract FileAttributes GetAttributes(ReadOnlySpan<char> path);

    static abstract void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes);

    static abstract DateTime GetCreationTimeUtc(ReadOnlySpan<char> path);

    static abstract DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path);

    static abstract DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path);

    static abstract void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value);

    static abstract void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value);

    static abstract void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value);

    static abstract long GetLength(ReadOnlySpan<char> path);

    static abstract void CreateDirectory(ReadOnlySpan<char> path);

    static abstract void DeleteDirectory(ReadOnlySpan<char> path, bool recursive);

    static abstract IFileSystemDirectoryEnumerator EnumerateDirectory(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind);

    static abstract bool TryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten);

    static abstract UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path);

    static abstract void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode);

    static abstract void Encrypt(ReadOnlySpan<char> path);

    static abstract void Decrypt(ReadOnlySpan<char> path);
}
