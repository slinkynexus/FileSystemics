using System.Runtime.InteropServices;
using FileSystemics.IO.Interop;

namespace FileSystemics.IO.Internal;

internal static class MacOSDriveInfo {
    internal static DriveType GetDriveType(ReadOnlySpan<char> name) {
        if (!name.SequenceEqual("/".AsSpan())) {
            return DriveType.NoRootDirectory;
        }

        return DriveType.Fixed;
    }

    internal static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        if (!TryGetFileSystemType(name, destination, out charsWritten)) {
            ThrowForStatfsError();
        }
    }

    internal static long GetAvailableFreeSpace(ReadOnlySpan<char> name) {
        InteropMacOs.Statfs stats = GetStatfs(name);
        return (long)stats.f_bsize * (long)stats.f_bavail;
    }

    internal static long GetTotalFreeSpace(ReadOnlySpan<char> name) {
        InteropMacOs.Statfs stats = GetStatfs(name);
        return (long)stats.f_bsize * (long)stats.f_bfree;
    }

    internal static long GetTotalSize(ReadOnlySpan<char> name) {
        InteropMacOs.Statfs stats = GetStatfs(name);
        return (long)stats.f_bsize * (long)stats.f_blocks;
    }

    private static unsafe InteropMacOs.Statfs GetStatfs(ReadOnlySpan<char> name) {
        return MacOSPathRules.WithNativePath(
            name,
            (_, utf8Path) => {
                fixed (byte* pathPtr = utf8Path) {
                    if (InteropMacOs.statfs((nint)pathPtr, out InteropMacOs.Statfs stats) != 0) {
                        ThrowForStatfsError();
                    }

                    return stats;
                }
            });
    }

    private static bool TryGetFileSystemType(
        ReadOnlySpan<char> name,
        Span<char> fileSystemDestination,
        out int fileSystemLength) {
        fileSystemLength = 0;
        string? fileSystemType = MacOSPathRules.WithNativePath(
            name,
            (_, utf8Path) => {
                unsafe {
                    fixed (byte* pathPtr = utf8Path) {
                        if (InteropMacOs.statfs((nint)pathPtr, out InteropMacOs.Statfs stats) != 0) {
                            return null;
                        }

                        ReadOnlySpan<char> type = stats.f_fstypename.AsSpan();
                        int end = type.Length;
                        while (end > 0 && type[end - 1] == '\0') {
                            end--;
                        }

                        return end == 0 ? null : type[..end].ToString();
                    }
                }
            });

        if (fileSystemType is null || fileSystemType.Length > fileSystemDestination.Length) {
            return false;
        }

        fileSystemType.AsSpan().CopyTo(fileSystemDestination);
        fileSystemLength = fileSystemType.Length;
        return true;
    }

    private static void ThrowForStatfsError() {
        int errno = Marshal.GetLastWin32Error();
        if (errno == 2) {
            throw new DriveNotFoundException();
        }

        throw InteropHelpers.CreateExceptionForErrno(errno);
    }
}
