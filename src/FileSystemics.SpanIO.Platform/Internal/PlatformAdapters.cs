using InternalDirectoryEntryKind = FileSystemics.IO.Internal.DirectoryEntryKind;
using InternalDriveEnumerationKind = FileSystemics.IO.Internal.DriveEnumerationKind;
using InternalDriveEnumerator = FileSystemics.IO.Internal.IDriveEnumerator;
using InternalDirectoryEnumerator = FileSystemics.IO.Internal.IFileSystemDirectoryEnumerator;

namespace FileSystemics.IO.PlatformHosts.Internal;

internal sealed class DirectoryEnumeratorAdapter(IFileSystemDirectoryEnumerator core) : InternalDirectoryEnumerator {
    public bool IsDirectory => core.IsDirectory;

    public bool MoveNext() => core.MoveNext();

    public bool TryGetCurrentEntryName(Span<char> destination, out int charsWritten) =>
        core.TryGetCurrentEntryName(destination, out charsWritten);

    public void Dispose() => core.Dispose();
}

internal sealed class DriveEnumeratorAdapter(IDriveEnumerator core) : InternalDriveEnumerator {
    public bool MoveNext() => core.MoveNext();

    public bool TryGetCurrentDrive(Span<char> destination, out int charsWritten) =>
        core.TryGetCurrentDrive(destination, out charsWritten);

    public void Dispose() => core.Dispose();
}

internal static class PlatformAdapterMaps {
    internal static InternalDirectoryEntryKind ToInternal(DirectoryEntryKind kind) => kind switch {
        DirectoryEntryKind.Files => InternalDirectoryEntryKind.Files,
        DirectoryEntryKind.Directories => InternalDirectoryEntryKind.Directories,
        DirectoryEntryKind.All => InternalDirectoryEntryKind.All,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    internal static DirectoryEntryKind ToPublic(InternalDirectoryEntryKind kind) => kind switch {
        InternalDirectoryEntryKind.Files => DirectoryEntryKind.Files,
        InternalDirectoryEntryKind.Directories => DirectoryEntryKind.Directories,
        InternalDirectoryEntryKind.All => DirectoryEntryKind.All,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    internal static DriveEnumerationKind ToPublic(InternalDriveEnumerationKind kind) => kind switch {
        InternalDriveEnumerationKind.WindowsLogicalDrives => DriveEnumerationKind.WindowsLogicalDrives,
        InternalDriveEnumerationKind.LinuxMounts => DriveEnumerationKind.LinuxMounts,
        InternalDriveEnumerationKind.MacMounts => DriveEnumerationKind.MacMounts,
        InternalDriveEnumerationKind.UnixSingleRoot => DriveEnumerationKind.UnixSingleRoot,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
