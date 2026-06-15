using System.Runtime.InteropServices;

namespace FileSystemics.IO.Interop;

internal static partial class InteropWindowsDrive {
    internal const int MAX_PATH = 260;

    internal const uint DRIVE_UNKNOWN = 0;
    internal const uint DRIVE_NO_ROOT_DIR = 1;
    internal const uint DRIVE_REMOVABLE = 2;
    internal const uint DRIVE_FIXED = 3;
    internal const uint DRIVE_REMOTE = 4;
    internal const uint DRIVE_CDROM = 5;
    internal const uint DRIVE_RAMDISK = 6;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial uint GetLogicalDrives();

    [LibraryImport("kernel32.dll", EntryPoint = "GetDriveTypeW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial uint GetDriveType(ReadOnlySpan<char> lpRootPathName);

    [LibraryImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetDiskFreeSpaceEx(
        ReadOnlySpan<char> lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool GetVolumeInformationW(
        ReadOnlySpan<char> lpRootPathName,
        Span<char> lpVolumeNameBuffer,
        uint nVolumeNameSize,
        nint lpVolumeSerialNumber,
        nint lpMaximumComponentLength,
        out uint lpFileSystemFlags,
        Span<char> lpFileSystemNameBuffer,
        uint nFileSystemNameSize);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetVolumeLabelW(ReadOnlySpan<char> lpRootPathName, ReadOnlySpan<char> lpVolumeName);
}
