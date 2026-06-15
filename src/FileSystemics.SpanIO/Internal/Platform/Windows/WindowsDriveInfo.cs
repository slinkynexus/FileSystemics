using System.Runtime.InteropServices;
using FileSystemics.IO.Interop;

namespace FileSystemics.IO.Internal;

internal static class WindowsDriveInfo {
    internal static DriveType GetDriveType(ReadOnlySpan<char> name) =>
        (DriveType)InteropWindowsDrive.GetDriveType(name);

    internal static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        Span<char> fileSystemName = stackalloc char[InteropWindowsDrive.MAX_PATH + 1];
        Span<char> volumeName = stackalloc char[InteropWindowsDrive.MAX_PATH + 1];
        unsafe {
            if (!InteropWindowsDrive.GetVolumeInformationW(
                    name,
                    volumeName,
                    (uint)volumeName.Length,
                    nint.Zero,
                    nint.Zero,
                    out _,
                    fileSystemName,
                    (uint)fileSystemName.Length)) {
                ThrowDriveError();
            }
        }

        int length = fileSystemName.IndexOf('\0');
        length = length < 0 ? fileSystemName.Length : length;
        if (length > destination.Length) {
            throw SpanIOException.DestinationTooSmall();
        }

        fileSystemName[..length].CopyTo(destination);
        charsWritten = length;
    }

    internal static void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        Span<char> volumeName = stackalloc char[InteropWindowsDrive.MAX_PATH + 1];
        unsafe {
            if (!InteropWindowsDrive.GetVolumeInformationW(
                    name,
                    volumeName,
                    (uint)volumeName.Length,
                    nint.Zero,
                    nint.Zero,
                    out _,
                    Span<char>.Empty,
                    0)) {
                ThrowDriveError();
            }
        }

        int length = volumeName.IndexOf('\0');
        length = length < 0 ? volumeName.Length : length;
        if (length > destination.Length) {
            throw SpanIOException.DestinationTooSmall();
        }

        volumeName[..length].CopyTo(destination);
        charsWritten = length;
    }

    internal static void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) {
        if (!InteropWindowsDrive.SetVolumeLabelW(name, label)) {
            int error = Marshal.GetLastWin32Error();
            if (error == 5) {
                throw new UnauthorizedAccessException("Access denied setting volume label.");
            }

            throw InteropHelpers.CreateExceptionForWin32Error(error);
        }
    }

    internal static long GetAvailableFreeSpace(ReadOnlySpan<char> name) {
        GetDiskSpace(name, out ulong available, out _, out _);
        return checked((long)available);
    }

    internal static long GetTotalFreeSpace(ReadOnlySpan<char> name) {
        GetDiskSpace(name, out _, out _, out ulong free);
        return checked((long)free);
    }

    internal static long GetTotalSize(ReadOnlySpan<char> name) {
        GetDiskSpace(name, out _, out ulong total, out _);
        return checked((long)total);
    }

    private static void GetDiskSpace(ReadOnlySpan<char> name, out ulong available, out ulong total, out ulong free) {
        if (!InteropWindowsDrive.GetDiskFreeSpaceEx(name, out available, out total, out free)) {
            ThrowDriveError();
        }
    }

    private static void ThrowDriveError() {
        throw InteropHelpers.CreateExceptionForWin32Error(Marshal.GetLastWin32Error());
    }
}
