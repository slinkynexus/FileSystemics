using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.Internal;

internal static unsafe class NativePlatformTable {
    internal static NativePlatformBindingSnapshot Active { get; private set; }
    internal static INativePathEncoding PathEncoding { get; set; } = NativePlatformPathEncoding.Instance;
    internal static char DirectorySeparatorChar => Active.DirectorySeparatorChar;
    internal static char AltDirectorySeparatorChar => Active.AltDirectorySeparatorChar;
    internal static char VolumeSeparatorChar => Active.VolumeSeparatorChar;
    internal static char PathSeparator => Active.PathSeparator;
    internal static StringComparison PathComparison => Active.PathComparison;
    internal static bool UsesUtf16NativePaths => Active.UsesUtf16NativePaths;
    internal static bool UsesGetdents64DirectoryEnumeration => Active.UsesGetdents64DirectoryEnumeration;
    internal static DriveEnumerationKind DriveEnumerationKind => Active.DriveEnumerationKind;
    internal static bool VolumeLabelIsMountPath => Active.VolumeLabelIsMountPath;
    internal static bool UsesDriveLetters => Active.UsesDriveLetters;

    static NativePlatformTable() {
        Active = OperatingSystem.IsWindows()
            ? PlatformTable<WindowsFileSystem, WindowsPathRules, WindowsDrivePlatform>.Value
            : OperatingSystem.IsMacOS()
                ? PlatformTable<MacOSFileSystem, MacOSPathRules, MacOSDrivePlatform>.Value
                : PlatformTable<LinuxFileSystem, LinuxPathRules, LinuxDrivePlatform>.Value;
    }

    internal static void SetActive(NativePlatformBindingSnapshot snapshot) => Active = snapshot;

    internal static bool InvokeExists(ReadOnlySpan<char> path) => Active.Exists(path);

    internal static bool InvokeDirectoryExists(ReadOnlySpan<char> path) => Active.DirectoryExists(path);

    internal static SafeFileHandle InvokeOpenHandle(
        ReadOnlySpan<char> path,
        FileMode mode,
        FileAccess access,
        FileShare share,
        FileOptions options) => Active.OpenHandle(path, mode, access, share, options);

    internal static SafeFileHandle InvokeOpenHandleCombined(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        FileMode mode,
        FileAccess access,
        FileShare share) => Active.OpenHandleCombined(directory, fileName, mode, access, share);

    internal static void InvokeDelete(ReadOnlySpan<char> path) => Active.Delete(path);

    internal static void InvokeCopy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite) =>
        Active.Copy(source, destination, overwrite);

    internal static void InvokeMove(ReadOnlySpan<char> source, ReadOnlySpan<char> destination) =>
        Active.Move(source, destination);

    internal static FileAttributes InvokeGetAttributes(ReadOnlySpan<char> path) => Active.GetAttributes(path);

    internal static void InvokeSetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) =>
        Active.SetAttributes(path, attributes);

    internal static DateTime InvokeGetCreationTimeUtc(ReadOnlySpan<char> path) => Active.GetCreationTimeUtc(path);

    internal static DateTime InvokeGetLastAccessTimeUtc(ReadOnlySpan<char> path) => Active.GetLastAccessTimeUtc(path);

    internal static DateTime InvokeGetLastWriteTimeUtc(ReadOnlySpan<char> path) => Active.GetLastWriteTimeUtc(path);

    internal static void InvokeSetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        Active.SetCreationTimeUtc(path, value);

    internal static void InvokeSetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        Active.SetLastAccessTimeUtc(path, value);

    internal static void InvokeSetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        Active.SetLastWriteTimeUtc(path, value);

    internal static long InvokeGetLength(ReadOnlySpan<char> path) => Active.GetLength(path);

    internal static void InvokeCreateDirectory(ReadOnlySpan<char> path) => Active.CreateDirectory(path);

    internal static void InvokeDeleteDirectory(ReadOnlySpan<char> path, bool recursive) =>
        Active.DeleteDirectory(path, recursive);

    internal static IFileSystemDirectoryEnumerator InvokeEnumerateDirectory(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind) => Active.EnumerateDirectory(directory, searchPattern, kind);

    internal static bool InvokeTryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) {
        fixed (int* written = &charsWritten) {
            return Active.TryGetLinkTarget(path, destination, written);
        }
    }

    internal static UnixFileMode InvokeGetUnixFileMode(ReadOnlySpan<char> path) => Active.GetUnixFileMode(path);

    internal static void InvokeSetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) =>
        Active.SetUnixFileMode(path, mode);

    internal static void InvokeEncrypt(ReadOnlySpan<char> path) => Active.Encrypt(path);

    internal static void InvokeDecrypt(ReadOnlySpan<char> path) => Active.Decrypt(path);

    internal static void InvokeGetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        fixed (int* written = &charsWritten) {
            Active.GetDriveFormat(name, destination, written);
        }
    }

    internal static void InvokeGetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        fixed (int* written = &charsWritten) {
            Active.GetVolumeLabel(name, destination, written);
        }
    }

    internal static int InvokeGetRootLength(ReadOnlySpan<char> path) => Active.GetRootLength(path);

    internal static bool InvokeIsPathRooted(ReadOnlySpan<char> path) => Active.IsPathRooted(path);

    internal static bool InvokeIsPartiallyQualified(ReadOnlySpan<char> path) => Active.IsPartiallyQualified(path);

    internal static bool InvokeIsDirectorySeparator(char value) => Active.IsDirectorySeparator(value);

    internal static bool InvokeShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path) =>
        Active.ShouldExistenceCheckReturnFalse(path);

    internal static bool InvokeIsEffectivelyEmpty(ReadOnlySpan<char> path) => Active.IsEffectivelyEmpty(path);

    internal static DriveType InvokeGetDriveType(ReadOnlySpan<char> name) => Active.GetDriveType(name);

    internal static void InvokeSetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) =>
        Active.SetVolumeLabel(name, label);

    internal static long InvokeGetAvailableFreeSpace(ReadOnlySpan<char> name) => Active.GetAvailableFreeSpace(name);

    internal static long InvokeGetTotalFreeSpace(ReadOnlySpan<char> name) => Active.GetTotalFreeSpace(name);

    internal static long InvokeGetTotalSize(ReadOnlySpan<char> name) => Active.GetTotalSize(name);

    internal static int InvokeGetNormalizedDriveCapacity(ReadOnlySpan<char> driveName, string paramName) =>
        Active.GetNormalizedDriveCapacity(driveName, paramName);

    internal static int InvokeNormalizeDriveName(ReadOnlySpan<char> driveName, Span<char> destination, string paramName) =>
        Active.NormalizeDriveName(driveName, destination, paramName);

    internal static IDriveEnumerator InvokeEnumerateDrives() => Active.EnumerateDrives();
}

internal static unsafe class PlatformTable<TFileSystem, TPathRules, TDrivePlatform>
    where TFileSystem : IFileSystemPlatform<TFileSystem>
    where TPathRules : INativePathRules<TPathRules>
    where TDrivePlatform : INativeDrivePlatform<TDrivePlatform> {
    internal static readonly NativePlatformBindingSnapshot Value = new() {
        Exists = &TFileSystem.Exists,
        DirectoryExists = &TFileSystem.DirectoryExists,
        OpenHandle = &TFileSystem.OpenHandle,
        OpenHandleCombined = &OpenHandleCombinedDispatch<TFileSystem>,
        Delete = &TFileSystem.Delete,
        Copy = &TFileSystem.Copy,
        Move = &TFileSystem.Move,
        GetAttributes = &TFileSystem.GetAttributes,
        SetAttributes = &TFileSystem.SetAttributes,
        GetCreationTimeUtc = &TFileSystem.GetCreationTimeUtc,
        GetLastAccessTimeUtc = &TFileSystem.GetLastAccessTimeUtc,
        GetLastWriteTimeUtc = &TFileSystem.GetLastWriteTimeUtc,
        SetCreationTimeUtc = &TFileSystem.SetCreationTimeUtc,
        SetLastAccessTimeUtc = &TFileSystem.SetLastAccessTimeUtc,
        SetLastWriteTimeUtc = &TFileSystem.SetLastWriteTimeUtc,
        GetLength = &TFileSystem.GetLength,
        CreateDirectory = &TFileSystem.CreateDirectory,
        DeleteDirectory = &TFileSystem.DeleteDirectory,
        EnumerateDirectory = &TFileSystem.EnumerateDirectory,
        TryGetLinkTarget = &TryGetLinkTargetDispatch<TFileSystem>,
        GetUnixFileMode = &TFileSystem.GetUnixFileMode,
        SetUnixFileMode = &TFileSystem.SetUnixFileMode,
        Encrypt = &TFileSystem.Encrypt,
        Decrypt = &TFileSystem.Decrypt,
        DirectorySeparatorChar = TPathRules.DirectorySeparatorChar,
        AltDirectorySeparatorChar = TPathRules.AltDirectorySeparatorChar,
        VolumeSeparatorChar = TPathRules.VolumeSeparatorChar,
        PathSeparator = TPathRules.PathSeparator,
        PathComparison = TPathRules.PathComparison,
        UsesUtf16NativePaths = TPathRules.UsesUtf16NativePaths,
        UsesGetdents64DirectoryEnumeration = TPathRules.UsesGetdents64DirectoryEnumeration,
        GetRootLength = &TPathRules.GetRootLength,
        IsPathRooted = &TPathRules.IsPathRooted,
        IsPartiallyQualified = &TPathRules.IsPartiallyQualified,
        IsDirectorySeparator = &TPathRules.IsDirectorySeparator,
        ShouldExistenceCheckReturnFalse = &TPathRules.ShouldExistenceCheckReturnFalse,
        IsEffectivelyEmpty = &TPathRules.IsEffectivelyEmpty,
        DriveEnumerationKind = TDrivePlatform.EnumerationKind,
        VolumeLabelIsMountPath = TDrivePlatform.VolumeLabelIsMountPath,
        UsesDriveLetters = TDrivePlatform.UsesDriveLetters,
        GetDriveType = &TDrivePlatform.GetDriveType,
        GetDriveFormat = &GetDriveFormatDispatch<TDrivePlatform>,
        GetVolumeLabel = &GetVolumeLabelDispatch<TDrivePlatform>,
        SetVolumeLabel = &TDrivePlatform.SetVolumeLabel,
        GetAvailableFreeSpace = &TDrivePlatform.GetAvailableFreeSpace,
        GetTotalFreeSpace = &TDrivePlatform.GetTotalFreeSpace,
        GetTotalSize = &TDrivePlatform.GetTotalSize,
        GetNormalizedDriveCapacity = &TDrivePlatform.GetNormalizedDriveCapacity,
        NormalizeDriveName = &NormalizeDriveNameDispatch<TDrivePlatform>,
        EnumerateDrives = &TDrivePlatform.EnumerateDrives,
    };

    private static SafeFileHandle OpenHandleCombinedDispatch<TDispatchFileSystem>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        FileMode mode,
        FileAccess access,
        FileShare share) where TDispatchFileSystem : IFileSystemPlatform<TDispatchFileSystem> =>
        TDispatchFileSystem.OpenHandle(directory, fileName, mode, access, share);

    private static bool TryGetLinkTargetDispatch<TDispatchFileSystem>(
        ReadOnlySpan<char> path,
        Span<char> destination,
        int* charsWritten) where TDispatchFileSystem : IFileSystemPlatform<TDispatchFileSystem> {
        int written;
        bool result = TDispatchFileSystem.TryGetLinkTarget(path, destination, out written);
        *charsWritten = written;
        return result;
    }

    private static void GetDriveFormatDispatch<TDispatchDrivePlatform>(
        ReadOnlySpan<char> name,
        Span<char> destination,
        int* charsWritten) where TDispatchDrivePlatform : INativeDrivePlatform<TDispatchDrivePlatform> {
        TDispatchDrivePlatform.GetDriveFormat(name, destination, out int written);
        *charsWritten = written;
    }

    private static void GetVolumeLabelDispatch<TDispatchDrivePlatform>(
        ReadOnlySpan<char> name,
        Span<char> destination,
        int* charsWritten) where TDispatchDrivePlatform : INativeDrivePlatform<TDispatchDrivePlatform> {
        TDispatchDrivePlatform.GetVolumeLabel(name, destination, out int written);
        *charsWritten = written;
    }

    private static int NormalizeDriveNameDispatch<TDispatchDrivePlatform>(
        ReadOnlySpan<char> driveName,
        Span<char> destination,
        string paramName) where TDispatchDrivePlatform : INativeDrivePlatform<TDispatchDrivePlatform> =>
        TDispatchDrivePlatform.NormalizeDriveName(driveName, destination, paramName);
}
