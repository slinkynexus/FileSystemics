using FileSystemics.IO.Internal;
using Microsoft.Win32.SafeHandles;
using InternalDriveEnumerator = FileSystemics.IO.Internal.IDriveEnumerator;
using InternalDirectoryEnumerator = FileSystemics.IO.Internal.IFileSystemDirectoryEnumerator;

namespace FileSystemics.IO.PlatformHosts.Internal;

internal sealed class NativePlatformHostFacade : IPlatformHost {
    internal static readonly NativePlatformHostFacade Instance = new();

    private NativePlatformHostFacade() {
    }

    public IFileSystemPlatform FileSystem { get; } = new NativeFileSystemFacade();

    public IPathRules Path { get; } = new NativePathRulesFacade();

    public IDrivePlatform Drives { get; } = new NativeDrivePlatformFacade();

    private sealed class NativeFileSystemFacade : IFileSystemPlatform {
        public bool Exists(ReadOnlySpan<char> path) => NativePlatformTable.InvokeExists(path);

        public bool DirectoryExists(ReadOnlySpan<char> path) => NativePlatformTable.InvokeDirectoryExists(path);

        public SafeFileHandle OpenHandle(
            ReadOnlySpan<char> path,
            FileMode mode,
            FileAccess access,
            FileShare share,
            FileOptions options) =>
            NativePlatformTable.InvokeOpenHandle(path, mode, access, share, options);

        public SafeFileHandle OpenHandle(
            ReadOnlySpan<char> directory,
            ReadOnlySpan<char> fileName,
            FileMode mode,
            FileAccess access,
            FileShare share) =>
            NativePlatformTable.InvokeOpenHandleCombined(directory, fileName, mode, access, share);

        public void Delete(ReadOnlySpan<char> path) => NativePlatformTable.InvokeDelete(path);

        public void Copy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite) =>
            NativePlatformTable.InvokeCopy(source, destination, overwrite);

        public void Move(ReadOnlySpan<char> source, ReadOnlySpan<char> destination) =>
            NativePlatformTable.InvokeMove(source, destination);

        public FileAttributes GetAttributes(ReadOnlySpan<char> path) => NativePlatformTable.InvokeGetAttributes(path);

        public void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) =>
            NativePlatformTable.InvokeSetAttributes(path, attributes);

        public DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) => NativePlatformTable.InvokeGetCreationTimeUtc(path);

        public DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) => NativePlatformTable.InvokeGetLastAccessTimeUtc(path);

        public DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) => NativePlatformTable.InvokeGetLastWriteTimeUtc(path);

        public void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
            NativePlatformTable.InvokeSetCreationTimeUtc(path, value);

        public void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
            NativePlatformTable.InvokeSetLastAccessTimeUtc(path, value);

        public void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
            NativePlatformTable.InvokeSetLastWriteTimeUtc(path, value);

        public long GetLength(ReadOnlySpan<char> path) => NativePlatformTable.InvokeGetLength(path);

        public void CreateDirectory(ReadOnlySpan<char> path) => NativePlatformTable.InvokeCreateDirectory(path);

        public void DeleteDirectory(ReadOnlySpan<char> path, bool recursive) =>
            NativePlatformTable.InvokeDeleteDirectory(path, recursive);

        public IFileSystemDirectoryEnumerator EnumerateDirectory(
            ReadOnlySpan<char> directory,
            ReadOnlySpan<char> searchPattern,
            DirectoryEntryKind kind) =>
            new PublicDirectoryEnumeratorAdapter(
                NativePlatformTable.InvokeEnumerateDirectory(
                    directory,
                    searchPattern,
                    PlatformAdapterMaps.ToInternal(kind)));

        public bool TryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) =>
            NativePlatformTable.InvokeTryGetLinkTarget(path, destination, out charsWritten);

        public UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) => NativePlatformTable.InvokeGetUnixFileMode(path);

        public void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) =>
            NativePlatformTable.InvokeSetUnixFileMode(path, mode);

        public void Encrypt(ReadOnlySpan<char> path) => NativePlatformTable.InvokeEncrypt(path);

        public void Decrypt(ReadOnlySpan<char> path) => NativePlatformTable.InvokeDecrypt(path);
    }

    private sealed class PublicDirectoryEnumeratorAdapter(InternalDirectoryEnumerator core) : IFileSystemDirectoryEnumerator {
        public bool IsDirectory => core.IsDirectory;

        public bool MoveNext() => core.MoveNext();

        public bool TryGetCurrentEntryName(Span<char> destination, out int charsWritten) =>
            core.TryGetCurrentEntryName(destination, out charsWritten);

        public void Dispose() => core.Dispose();
    }

    private sealed class NativePathRulesFacade : IPathRules {
        public char DirectorySeparatorChar => NativePlatformTable.DirectorySeparatorChar;

        public char AltDirectorySeparatorChar => NativePlatformTable.AltDirectorySeparatorChar;

        public char VolumeSeparatorChar => NativePlatformTable.VolumeSeparatorChar;

        public char PathSeparator => NativePlatformTable.PathSeparator;

        public StringComparison PathComparison => NativePlatformTable.PathComparison;

        public bool UsesUtf16NativePaths => NativePlatformTable.UsesUtf16NativePaths;

        public bool UsesGetdents64DirectoryEnumeration => NativePlatformTable.UsesGetdents64DirectoryEnumeration;

        public int GetRootLength(ReadOnlySpan<char> path) => NativePlatformTable.InvokeGetRootLength(path);

        public bool IsPathRooted(ReadOnlySpan<char> path) => NativePlatformTable.InvokeIsPathRooted(path);

        public bool IsPartiallyQualified(ReadOnlySpan<char> path) => NativePlatformTable.InvokeIsPartiallyQualified(path);

        public bool IsDirectorySeparator(char value) => NativePlatformTable.InvokeIsDirectorySeparator(value);

        public bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path) =>
            NativePlatformTable.InvokeShouldExistenceCheckReturnFalse(path);

        public bool IsEffectivelyEmpty(ReadOnlySpan<char> path) => NativePlatformTable.InvokeIsEffectivelyEmpty(path);

        public T WithNativePath<T>(
            ReadOnlySpan<char> path,
            Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
            NativePlatformTable.PathEncoding.WithNativePath(path, action);

        public T WithCombinedNativePath<T>(
            ReadOnlySpan<char> directory,
            ReadOnlySpan<char> fileName,
            char separator,
            Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
            NativePlatformTable.PathEncoding.WithCombinedNativePath(directory, fileName, separator, action);

        public T WithTwoNativePaths<T>(
            ReadOnlySpan<char> path1,
            ReadOnlySpan<char> path2,
            Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
            Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) =>
            NativePlatformTable.PathEncoding.WithTwoNativePaths(path1, path2, utf16Action, utf8Action);
    }

    private sealed class NativeDrivePlatformFacade : IDrivePlatform {
        public DriveEnumerationKind EnumerationKind =>
            PlatformAdapterMaps.ToPublic(NativePlatformTable.DriveEnumerationKind);

        public bool UsesDriveLetters => NativePlatformTable.UsesDriveLetters;

        public bool VolumeLabelIsMountPath => NativePlatformTable.VolumeLabelIsMountPath;

        public int GetNormalizedDriveCapacity(ReadOnlySpan<char> driveName, string paramName) =>
            NativePlatformTable.InvokeGetNormalizedDriveCapacity(driveName, paramName);

        public int NormalizeDriveName(ReadOnlySpan<char> driveName, Span<char> destination, string paramName) =>
            NativePlatformTable.InvokeNormalizeDriveName(driveName, destination, paramName);

        public DriveType GetDriveType(ReadOnlySpan<char> name) => NativePlatformTable.InvokeGetDriveType(name);

        public void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
            NativePlatformTable.InvokeGetDriveFormat(name, destination, out charsWritten);

        public void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
            NativePlatformTable.InvokeGetVolumeLabel(name, destination, out charsWritten);

        public void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) =>
            NativePlatformTable.InvokeSetVolumeLabel(name, label);

        public long GetAvailableFreeSpace(ReadOnlySpan<char> name) => NativePlatformTable.InvokeGetAvailableFreeSpace(name);

        public long GetTotalFreeSpace(ReadOnlySpan<char> name) => NativePlatformTable.InvokeGetTotalFreeSpace(name);

        public long GetTotalSize(ReadOnlySpan<char> name) => NativePlatformTable.InvokeGetTotalSize(name);

        public IDriveEnumerator EnumerateDrives() =>
            new PublicDriveEnumeratorAdapter(NativePlatformTable.InvokeEnumerateDrives());
    }

    private sealed class PublicDriveEnumeratorAdapter(InternalDriveEnumerator core) : IDriveEnumerator {
        public bool MoveNext() => core.MoveNext();

        public bool TryGetCurrentDrive(Span<char> destination, out int charsWritten) =>
            core.TryGetCurrentDrive(destination, out charsWritten);

        public void Dispose() => core.Dispose();
    }
}
