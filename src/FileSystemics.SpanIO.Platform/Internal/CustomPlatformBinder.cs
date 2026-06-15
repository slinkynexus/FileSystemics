using FileSystemics.IO.Internal;
using InternalDirectoryEntryKind = FileSystemics.IO.Internal.DirectoryEntryKind;
using InternalDriveEnumerationKind = FileSystemics.IO.Internal.DriveEnumerationKind;
using InternalDriveEnumerator = FileSystemics.IO.Internal.IDriveEnumerator;
using InternalDirectoryEnumerator = FileSystemics.IO.Internal.IFileSystemDirectoryEnumerator;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.PlatformHosts.Internal;

internal static unsafe class CustomPlatformBinder {
    private static readonly Lock s_sync = new();
    private static readonly Stack<IPlatformHost> s_hostStack = new();
    private static NativePlatformBindingSnapshot s_nativeSnapshot;
    private static INativePathEncoding? s_nativePathEncoding;
    private static bool s_nativeCaptured;

    internal static void Bind(IPlatformHost host) {
        ArgumentNullException.ThrowIfNull(host);

        lock (s_sync) {
            BindCore(host);
        }
    }

    internal static void Unbind() {
        lock (s_sync) {
            UnbindCore();
        }
    }

    private static void BindCore(IPlatformHost host) {
        if (!s_nativeCaptured) {
            s_nativeSnapshot = NativePlatformBinding.Capture();
            s_nativePathEncoding = NativePlatformTable.PathEncoding;
            s_nativeCaptured = true;
        }

        s_hostStack.Push(host);
        ApplyHost(host);
    }

    private static void UnbindCore() {
        if (s_hostStack.Count == 0) {
            return;
        }

        s_hostStack.Pop();
        if (s_hostStack.TryPeek(out IPlatformHost? host)) {
            ApplyHost(host);
            return;
        }

        NativePlatformBinding.Restore(s_nativeSnapshot);
        NativePlatformTable.PathEncoding = s_nativePathEncoding ?? NativePlatformPathEncoding.Instance;
    }

    private static void ApplyHost(IPlatformHost host) {
        NativePlatformBinding.Apply(CreateSnapshot(host));
        NativePlatformTable.PathEncoding = new HostPathEncodingAdapter(host.Path);
    }

    private static NativePlatformBindingSnapshot CreateSnapshot(IPlatformHost host) =>
        new() {
            Exists = &Exists,
            DirectoryExists = &DirectoryExists,
            OpenHandle = &OpenHandle,
            OpenHandleCombined = &OpenHandleCombined,
            Delete = &Delete,
            Copy = &Copy,
            Move = &Move,
            GetAttributes = &GetAttributes,
            SetAttributes = &SetAttributes,
            GetCreationTimeUtc = &GetCreationTimeUtc,
            GetLastAccessTimeUtc = &GetLastAccessTimeUtc,
            GetLastWriteTimeUtc = &GetLastWriteTimeUtc,
            SetCreationTimeUtc = &SetCreationTimeUtc,
            SetLastAccessTimeUtc = &SetLastAccessTimeUtc,
            SetLastWriteTimeUtc = &SetLastWriteTimeUtc,
            GetLength = &GetLength,
            CreateDirectory = &CreateDirectory,
            DeleteDirectory = &DeleteDirectory,
            EnumerateDirectory = &EnumerateDirectory,
            TryGetLinkTarget = &TryGetLinkTarget,
            GetUnixFileMode = &GetUnixFileMode,
            SetUnixFileMode = &SetUnixFileMode,
            Encrypt = &Encrypt,
            Decrypt = &Decrypt,
            DirectorySeparatorChar = host.Path.DirectorySeparatorChar,
            AltDirectorySeparatorChar = host.Path.AltDirectorySeparatorChar,
            VolumeSeparatorChar = host.Path.VolumeSeparatorChar,
            PathSeparator = host.Path.PathSeparator,
            PathComparison = host.Path.PathComparison,
            UsesUtf16NativePaths = host.Path.UsesUtf16NativePaths,
            UsesGetdents64DirectoryEnumeration = host.Path.UsesGetdents64DirectoryEnumeration,
            DriveEnumerationKind = ToInternalDriveEnumerationKind(host.Drives.EnumerationKind),
            VolumeLabelIsMountPath = host.Drives.VolumeLabelIsMountPath,
            UsesDriveLetters = host.Drives.UsesDriveLetters,
            GetRootLength = &GetRootLength,
            IsPathRooted = &IsPathRooted,
            IsPartiallyQualified = &IsPartiallyQualified,
            IsDirectorySeparator = &IsDirectorySeparator,
            ShouldExistenceCheckReturnFalse = &ShouldExistenceCheckReturnFalse,
            IsEffectivelyEmpty = &IsEffectivelyEmpty,
            GetDriveType = &GetDriveType,
            GetDriveFormat = &GetDriveFormat,
            GetVolumeLabel = &GetVolumeLabel,
            SetVolumeLabel = &SetVolumeLabel,
            GetAvailableFreeSpace = &GetAvailableFreeSpace,
            GetTotalFreeSpace = &GetTotalFreeSpace,
            GetTotalSize = &GetTotalSize,
            GetNormalizedDriveCapacity = &GetNormalizedDriveCapacity,
            NormalizeDriveName = &NormalizeDriveName,
            EnumerateDrives = &EnumerateDrives,
        };

    private static InternalDriveEnumerationKind ToInternalDriveEnumerationKind(DriveEnumerationKind kind) => kind switch {
        DriveEnumerationKind.WindowsLogicalDrives => InternalDriveEnumerationKind.WindowsLogicalDrives,
        DriveEnumerationKind.LinuxMounts => InternalDriveEnumerationKind.LinuxMounts,
        DriveEnumerationKind.MacMounts => InternalDriveEnumerationKind.MacMounts,
        DriveEnumerationKind.UnixSingleRoot => InternalDriveEnumerationKind.UnixSingleRoot,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static IPlatformHost Host =>
        s_hostStack.Peek();

    private static bool Exists(ReadOnlySpan<char> path) => Host.FileSystem.Exists(path);

    private static bool DirectoryExists(ReadOnlySpan<char> path) => Host.FileSystem.DirectoryExists(path);

    private static SafeFileHandle OpenHandle(
        ReadOnlySpan<char> path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options) =>
        Host.FileSystem.OpenHandle(path, mode, access, share, options);

    private static SafeFileHandle OpenHandleCombined(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        FileMode mode,
        FileAccess access,
        FileShare share) =>
        Host.FileSystem.OpenHandle(directory, fileName, mode, access, share);

    private static void Delete(ReadOnlySpan<char> path) => Host.FileSystem.Delete(path);

    private static void Copy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite) =>
        Host.FileSystem.Copy(source, destination, overwrite);

    private static void Move(ReadOnlySpan<char> source, ReadOnlySpan<char> destination) =>
        Host.FileSystem.Move(source, destination);

    private static FileAttributes GetAttributes(ReadOnlySpan<char> path) => Host.FileSystem.GetAttributes(path);

    private static void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) =>
        Host.FileSystem.SetAttributes(path, attributes);

    private static DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) => Host.FileSystem.GetCreationTimeUtc(path);

    private static DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) => Host.FileSystem.GetLastAccessTimeUtc(path);

    private static DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) => Host.FileSystem.GetLastWriteTimeUtc(path);

    private static void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        Host.FileSystem.SetCreationTimeUtc(path, value);

    private static void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        Host.FileSystem.SetLastAccessTimeUtc(path, value);

    private static void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        Host.FileSystem.SetLastWriteTimeUtc(path, value);

    private static long GetLength(ReadOnlySpan<char> path) => Host.FileSystem.GetLength(path);

    private static void CreateDirectory(ReadOnlySpan<char> path) => Host.FileSystem.CreateDirectory(path);

    private static void DeleteDirectory(ReadOnlySpan<char> path, bool recursive) =>
        Host.FileSystem.DeleteDirectory(path, recursive);

    private static InternalDirectoryEnumerator EnumerateDirectory(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        InternalDirectoryEntryKind kind) =>
        new DirectoryEnumeratorAdapter(
            Host.FileSystem.EnumerateDirectory(directory, searchPattern, PlatformAdapterMaps.ToPublic(kind)));

    private static bool TryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, int* charsWritten) {
        bool result = Host.FileSystem.TryGetLinkTarget(path, destination, out int written);
        *charsWritten = written;
        return result;
    }

    private static UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) => Host.FileSystem.GetUnixFileMode(path);

    private static void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) =>
        Host.FileSystem.SetUnixFileMode(path, mode);

    private static void Encrypt(ReadOnlySpan<char> path) => Host.FileSystem.Encrypt(path);

    private static void Decrypt(ReadOnlySpan<char> path) => Host.FileSystem.Decrypt(path);

    private static int GetRootLength(ReadOnlySpan<char> path) => Host.Path.GetRootLength(path);

    private static bool IsPathRooted(ReadOnlySpan<char> path) => Host.Path.IsPathRooted(path);

    private static bool IsPartiallyQualified(ReadOnlySpan<char> path) => Host.Path.IsPartiallyQualified(path);

    private static bool IsDirectorySeparator(char value) => Host.Path.IsDirectorySeparator(value);

    private static bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path) =>
        Host.Path.ShouldExistenceCheckReturnFalse(path);

    private static bool IsEffectivelyEmpty(ReadOnlySpan<char> path) => Host.Path.IsEffectivelyEmpty(path);

    private static DriveType GetDriveType(ReadOnlySpan<char> name) => Host.Drives.GetDriveType(name);

    private static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, int* charsWritten) {
        Host.Drives.GetDriveFormat(name, destination, out int written);
        *charsWritten = written;
    }

    private static void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, int* charsWritten) {
        Host.Drives.GetVolumeLabel(name, destination, out int written);
        *charsWritten = written;
    }

    private static void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) =>
        Host.Drives.SetVolumeLabel(name, label);

    private static long GetAvailableFreeSpace(ReadOnlySpan<char> name) => Host.Drives.GetAvailableFreeSpace(name);

    private static long GetTotalFreeSpace(ReadOnlySpan<char> name) => Host.Drives.GetTotalFreeSpace(name);

    private static long GetTotalSize(ReadOnlySpan<char> name) => Host.Drives.GetTotalSize(name);

    private static int GetNormalizedDriveCapacity(ReadOnlySpan<char> driveName, string paramName) =>
        Host.Drives.GetNormalizedDriveCapacity(driveName, paramName);

    private static int NormalizeDriveName(ReadOnlySpan<char> driveName, Span<char> destination, string paramName) =>
        Host.Drives.NormalizeDriveName(driveName, destination, paramName);

    private static InternalDriveEnumerator EnumerateDrives() =>
        new DriveEnumeratorAdapter(Host.Drives.EnumerateDrives());
}
