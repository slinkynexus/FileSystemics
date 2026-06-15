using System.Runtime.InteropServices;
using FileSystemics.IO.Interop;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.Internal;

internal static class PosixFileSystem {
    internal static SafeFileHandle OpenHandleCore(ReadOnlySpan<byte> utf8Path, FileMode mode, FileAccess access, FileShare share) {
        int flags = PosixOpenFlags.O_RDONLY;
        if (access.HasFlag(FileAccess.Write)) {
            flags = access.HasFlag(FileAccess.Read) ? PosixOpenFlags.O_RDWR : PosixOpenFlags.O_WRONLY;
        }

        int creationFlags = mode switch {
            FileMode.Create => PosixOpenFlags.Creat | PosixOpenFlags.Trunc,
            FileMode.CreateNew => PosixOpenFlags.Creat | PosixOpenFlags.Excl,
            FileMode.OpenOrCreate => PosixOpenFlags.Creat,
            FileMode.Truncate => PosixOpenFlags.Trunc,
            FileMode.Append => PosixOpenFlags.Creat | PosixOpenFlags.O_WRONLY,
            _ => 0,
        };

        int fd = InteropUnix.open(utf8Path, flags | creationFlags, 0x1B6);
        if (fd < 0) {
            InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
        }

        if (OperatingSystem.IsMacOS() && (creationFlags & PosixOpenFlags.Creat) != 0) {
            InteropUnix.fchmod(fd, 0x1B6);
        }

        if (mode == FileMode.Append) {
            InteropUnix.lseek(fd, 0, 2);
        }

        return new SafeFileHandle((nint)fd, ownsHandle: true);
    }

    internal static void CopyByReadWrite(SafeFileHandle sourceHandle, SafeFileHandle destHandle) {
        int inFd = sourceHandle.DangerousGetHandle().ToInt32();
        int outFd = destHandle.DangerousGetHandle().ToInt32();
        InteropUnix.lseek(inFd, 0, 0);
        Span<byte> buffer = stackalloc byte[81920];
        unsafe {
            fixed (byte* ptr = buffer) {
                nint read;
                while ((read = InteropUnix.read(inFd, ptr, (nuint)buffer.Length)) > 0) {
                    nint written = 0;
                    while (written < read) {
                        nint w = InteropUnix.write(outFd, ptr + written, (nuint)(read - written));
                        if (w < 0) {
                            InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                        }

                        written += w;
                    }
                }

                if (read < 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }
            }
        }
    }

    internal static FileAttributes ModeToFileAttributes(int mode) {
        FileAttributes attributes = FileAttributes.Normal;
        if ((mode & InteropConstants.S_IFMT) == InteropConstants.S_IFDIR) {
            attributes |= FileAttributes.Directory;
        }

        if ((mode & 0x200) == 0) {
            attributes |= FileAttributes.ReadOnly;
        }

        if ((mode & InteropConstants.S_IFMT) == InteropConstants.S_IFLNK) {
            attributes |= FileAttributes.ReparsePoint;
        }

        return attributes;
    }

    internal static InteropUnix.Timespec ToTimespec(DateTime utc) {
        long ticks = (utc - DateTime.UnixEpoch).Ticks;
        return new InteropUnix.Timespec {
            tv_sec = ticks / TimeSpan.TicksPerSecond,
            tv_nsec = ticks % TimeSpan.TicksPerSecond * 100,
        };
    }

    internal static bool MatchesPatternString(string name, string pattern) {
        if (pattern.Length == 0 || pattern == "*") {
            return true;
        }

        if (pattern.StartsWith('*')) {
            ReadOnlySpan<char> suffix = pattern.AsSpan(1);
            return name.AsSpan().EndsWith(suffix);
        }

        if (pattern.EndsWith('*')) {
            ReadOnlySpan<char> prefix = pattern.AsSpan(0, pattern.Length - 1);
            return name.AsSpan().StartsWith(prefix);
        }

        return name == pattern;
    }
}
