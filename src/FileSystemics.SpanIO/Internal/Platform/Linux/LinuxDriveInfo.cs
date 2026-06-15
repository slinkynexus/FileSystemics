using System.Buffers;
using System.Runtime.InteropServices;
using FileSystemics.IO.Interop;

namespace FileSystemics.IO.Internal;

internal static class LinuxDriveInfo {
    private const int MountEntryBufferLength = 4096;

    internal static DriveType GetDriveType(ReadOnlySpan<char> name) {
        Span<char> fileSystemType = stackalloc char[256];
        if (!TryGetFileSystemType(name, fileSystemType, out int fileSystemLength)) {
            return DriveType.NoRootDirectory;
        }

        ReadOnlySpan<char> type = fileSystemType[..fileSystemLength];
        if (IsNetworkFileSystem(type)) {
            return DriveType.Network;
        }

        if (type.SequenceEqual("tmpfs".AsSpan()) || type.SequenceEqual("ramfs".AsSpan())) {
            return DriveType.Ram;
        }

        if (type.SequenceEqual("iso9660".AsSpan()) || type.SequenceEqual("udf".AsSpan())) {
            return DriveType.CDRom;
        }

        return DriveType.Fixed;
    }

    internal static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        if (!TryGetFileSystemType(name, destination, out charsWritten)) {
            ThrowForStatvfsError();
        }
    }

    internal static long GetAvailableFreeSpace(ReadOnlySpan<char> name) {
        InteropUnixDrive.Statvfs stats = GetStatvfs(name);
        return (long)(stats.f_frsize * stats.f_bavail);
    }

    internal static long GetTotalFreeSpace(ReadOnlySpan<char> name) {
        InteropUnixDrive.Statvfs stats = GetStatvfs(name);
        return (long)(stats.f_frsize * stats.f_bfree);
    }

    internal static long GetTotalSize(ReadOnlySpan<char> name) {
        InteropUnixDrive.Statvfs stats = GetStatvfs(name);
        return (long)(stats.f_frsize * stats.f_blocks);
    }

    private static unsafe InteropUnixDrive.Statvfs GetStatvfs(ReadOnlySpan<char> name) {
        return LinuxPathRules.WithNativePath(
            name,
            (_, utf8Path) => {
                fixed (byte* pathPtr = utf8Path) {
                    if (InteropUnixDrive.statvfs((nint)pathPtr, out InteropUnixDrive.Statvfs stats) != 0) {
                        ThrowForStatvfsError();
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
        if (!File.Exists("/proc/mounts")) {
            return false;
        }

        char[] mountPointBuffer = ArrayPool<char>.Shared.Rent(MountEntryBufferLength);
        char[] fileSystemBuffer = ArrayPool<char>.Shared.Rent(MountEntryBufferLength);
        try {
            using FileStream stream = File.OpenRead("/proc/mounts");
            byte[] byteBuffer = ArrayPool<byte>.Shared.Rent(4096);
            try {
                int bestMatchLength = -1;
                int bestFileSystemLength = 0;
                Span<byte> lineBuffer = stackalloc byte[1024];
                int byteLength = 0;
                int bytePosition = 0;

                while (TryReadMountLine(stream, byteBuffer, ref byteLength, ref bytePosition, lineBuffer, out int lineLength)) {
                    if (!LinuxMountEntryParser.TryGetMountEntry(
                            lineBuffer[..lineLength],
                            mountPointBuffer,
                            out int mountPointLength,
                            fileSystemBuffer,
                            out int currentFileSystemLength)) {
                        continue;
                    }

                    ReadOnlySpan<char> mountPoint = mountPointBuffer.AsSpan(0, mountPointLength);
                    if (!IsMountMatch(name, mountPoint) || mountPointLength <= bestMatchLength) {
                        continue;
                    }

                    bestMatchLength = mountPointLength;
                    bestFileSystemLength = currentFileSystemLength;
                    if (bestFileSystemLength > fileSystemDestination.Length) {
                        return false;
                    }

                    fileSystemBuffer.AsSpan(0, bestFileSystemLength).CopyTo(fileSystemDestination);
                }

                if (bestMatchLength < 0) {
                    return false;
                }

                fileSystemLength = bestFileSystemLength;
                return true;
            }
            finally {
                ArrayPool<byte>.Shared.Return(byteBuffer);
            }
        }
        finally {
            ArrayPool<char>.Shared.Return(mountPointBuffer);
            ArrayPool<char>.Shared.Return(fileSystemBuffer);
        }
    }

    private static bool IsMountMatch(ReadOnlySpan<char> name, ReadOnlySpan<char> mountPoint) {
        if (name.Length == mountPoint.Length) {
            return name.SequenceEqual(mountPoint);
        }

        return name.Length > mountPoint.Length &&
               name.StartsWith(mountPoint) &&
               name[mountPoint.Length] == '/';
    }

    private static bool IsNetworkFileSystem(ReadOnlySpan<char> type) =>
        type.SequenceEqual("nfs".AsSpan()) ||
        type.SequenceEqual("nfs4".AsSpan()) ||
        type.SequenceEqual("cifs".AsSpan()) ||
        type.SequenceEqual("smbfs".AsSpan()) ||
        type.SequenceEqual("fuse.sshfs".AsSpan());

    private static bool TryReadMountLine(
        FileStream stream,
        byte[] byteBuffer,
        ref int byteLength,
        ref int bytePosition,
        Span<byte> lineBuffer,
        out int lineLength) {
        lineLength = 0;
        while (true) {
            if (bytePosition >= byteLength) {
                int read = stream.Read(byteBuffer, 0, byteBuffer.Length);
                if (read == 0) {
                    return lineLength > 0;
                }

                byteLength = read;
                bytePosition = 0;
            }

            byte value = byteBuffer[bytePosition++];
            if (value == (byte)'\n') {
                return lineLength > 0;
            }

            if (lineLength < lineBuffer.Length) {
                lineBuffer[lineLength++] = value;
            }
        }
    }

    private static void ThrowForStatvfsError() {
        int errno = Marshal.GetLastWin32Error();
        if (errno == 2) {
            throw new DriveNotFoundException();
        }

        throw InteropHelpers.CreateExceptionForErrno(errno);
    }
}
