using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.Internal;

internal unsafe struct NativePlatformBindingSnapshot {
    internal delegate*<ReadOnlySpan<char>, bool> Exists;
    internal delegate*<ReadOnlySpan<char>, bool> DirectoryExists;
    internal delegate*<ReadOnlySpan<char>, FileMode, FileAccess, FileShare, FileOptions, SafeFileHandle> OpenHandle;
    internal delegate*<ReadOnlySpan<char>, ReadOnlySpan<char>, FileMode, FileAccess, FileShare, SafeFileHandle> OpenHandleCombined;
    internal delegate*<ReadOnlySpan<char>, void> Delete;
    internal delegate*<ReadOnlySpan<char>, ReadOnlySpan<char>, bool, void> Copy;
    internal delegate*<ReadOnlySpan<char>, ReadOnlySpan<char>, void> Move;
    internal delegate*<ReadOnlySpan<char>, FileAttributes> GetAttributes;
    internal delegate*<ReadOnlySpan<char>, FileAttributes, void> SetAttributes;
    internal delegate*<ReadOnlySpan<char>, DateTime> GetCreationTimeUtc;
    internal delegate*<ReadOnlySpan<char>, DateTime> GetLastAccessTimeUtc;
    internal delegate*<ReadOnlySpan<char>, DateTime> GetLastWriteTimeUtc;
    internal delegate*<ReadOnlySpan<char>, DateTime, void> SetCreationTimeUtc;
    internal delegate*<ReadOnlySpan<char>, DateTime, void> SetLastAccessTimeUtc;
    internal delegate*<ReadOnlySpan<char>, DateTime, void> SetLastWriteTimeUtc;
    internal delegate*<ReadOnlySpan<char>, long> GetLength;
    internal delegate*<ReadOnlySpan<char>, void> CreateDirectory;
    internal delegate*<ReadOnlySpan<char>, bool, void> DeleteDirectory;
    internal delegate*<ReadOnlySpan<char>, ReadOnlySpan<char>, DirectoryEntryKind, IFileSystemDirectoryEnumerator> EnumerateDirectory;
    internal delegate*<ReadOnlySpan<char>, Span<char>, int*, bool> TryGetLinkTarget;
    internal delegate*<ReadOnlySpan<char>, UnixFileMode> GetUnixFileMode;
    internal delegate*<ReadOnlySpan<char>, UnixFileMode, void> SetUnixFileMode;
    internal delegate*<ReadOnlySpan<char>, void> Encrypt;
    internal delegate*<ReadOnlySpan<char>, void> Decrypt;

    internal char DirectorySeparatorChar;
    internal char AltDirectorySeparatorChar;
    internal char VolumeSeparatorChar;
    internal char PathSeparator;
    internal StringComparison PathComparison;
    internal bool UsesUtf16NativePaths;
    internal bool UsesGetdents64DirectoryEnumeration;
    internal DriveEnumerationKind DriveEnumerationKind;
    internal bool VolumeLabelIsMountPath;
    internal bool UsesDriveLetters;

    internal delegate*<ReadOnlySpan<char>, int> GetRootLength;
    internal delegate*<ReadOnlySpan<char>, bool> IsPathRooted;
    internal delegate*<ReadOnlySpan<char>, bool> IsPartiallyQualified;
    internal delegate*<char, bool> IsDirectorySeparator;
    internal delegate*<ReadOnlySpan<char>, bool> ShouldExistenceCheckReturnFalse;
    internal delegate*<ReadOnlySpan<char>, bool> IsEffectivelyEmpty;

    internal delegate*<ReadOnlySpan<char>, DriveType> GetDriveType;
    internal delegate*<ReadOnlySpan<char>, Span<char>, int*, void> GetDriveFormat;
    internal delegate*<ReadOnlySpan<char>, Span<char>, int*, void> GetVolumeLabel;
    internal delegate*<ReadOnlySpan<char>, ReadOnlySpan<char>, void> SetVolumeLabel;
    internal delegate*<ReadOnlySpan<char>, long> GetAvailableFreeSpace;
    internal delegate*<ReadOnlySpan<char>, long> GetTotalFreeSpace;
    internal delegate*<ReadOnlySpan<char>, long> GetTotalSize;
    internal delegate*<ReadOnlySpan<char>, string, int> GetNormalizedDriveCapacity;
    internal delegate*<ReadOnlySpan<char>, Span<char>, string, int> NormalizeDriveName;
    internal delegate*<IDriveEnumerator> EnumerateDrives;
}

internal static unsafe class NativePlatformBinding {
    internal static NativePlatformBindingSnapshot Capture() => NativePlatformTable.Active;

    internal static void Apply(NativePlatformBindingSnapshot snapshot) =>
        NativePlatformTable.SetActive(snapshot);

    internal static void Restore(NativePlatformBindingSnapshot snapshot) => Apply(snapshot);
}
