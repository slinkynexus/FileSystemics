using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.Internal;

internal static class PlatformOps {
    internal static bool Exists(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeExists(path);

    internal static bool DirectoryExists(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeDirectoryExists(path);

    internal static SafeFileHandle OpenHandle(
        ReadOnlySpan<char> path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options) =>
        NativePlatformTable.InvokeOpenHandle(path, mode, access, share, options);

    internal static SafeFileHandle OpenHandle(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        FileMode mode,
        FileAccess access,
        FileShare share) =>
        NativePlatformTable.InvokeOpenHandleCombined(directory, fileName, mode, access, share);

    internal static void Delete(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeDelete(path);

    internal static void Copy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite) =>
        NativePlatformTable.InvokeCopy(source, destination, overwrite);

    internal static void Move(ReadOnlySpan<char> source, ReadOnlySpan<char> destination) =>
        NativePlatformTable.InvokeMove(source, destination);

    internal static FileAttributes GetAttributes(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeGetAttributes(path);

    internal static void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) =>
        NativePlatformTable.InvokeSetAttributes(path, attributes);

    internal static DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeGetCreationTimeUtc(path);

    internal static DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeGetLastAccessTimeUtc(path);

    internal static DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeGetLastWriteTimeUtc(path);

    internal static void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        NativePlatformTable.InvokeSetCreationTimeUtc(path, value);

    internal static void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        NativePlatformTable.InvokeSetLastAccessTimeUtc(path, value);

    internal static void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        NativePlatformTable.InvokeSetLastWriteTimeUtc(path, value);

    internal static long GetLength(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeGetLength(path);

    internal static void CreateDirectory(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeCreateDirectory(path);

    internal static void DeleteDirectory(ReadOnlySpan<char> path, bool recursive) =>
        NativePlatformTable.InvokeDeleteDirectory(path, recursive);

    internal static IFileSystemDirectoryEnumerator EnumerateDirectory(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind) =>
        NativePlatformTable.InvokeEnumerateDirectory(directory, searchPattern, kind);

    internal static bool TryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) =>
        NativePlatformTable.InvokeTryGetLinkTarget(path, destination, out charsWritten);

    internal static UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeGetUnixFileMode(path);

    internal static void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) =>
        NativePlatformTable.InvokeSetUnixFileMode(path, mode);

    internal static void Encrypt(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeEncrypt(path);

    internal static void Decrypt(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeDecrypt(path);
}
