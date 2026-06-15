using System.Runtime.InteropServices;

namespace FileSystemics.IO.Interop;

internal static unsafe partial class InteropUnix {
    [LibraryImport("libc", SetLastError = true)]
    internal static partial int open(ReadOnlySpan<byte> pathname, int flags, int mode);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int openat(int dirfd, ReadOnlySpan<byte> pathname, int flags, int mode);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int close(int fd);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int unlink(ReadOnlySpan<byte> pathname);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int unlinkat(int dirfd, ReadOnlySpan<byte> pathname, int flags);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int mkdir(ReadOnlySpan<byte> pathname, int mode);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int mkdirat(int dirfd, ReadOnlySpan<byte> pathname, int mode);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int rename(ReadOnlySpan<byte> oldpath, ReadOnlySpan<byte> newpath);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int access(ReadOnlySpan<byte> pathname, int mode);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int chmod(ReadOnlySpan<byte> pathname, int mode);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int fchmod(int fd, int mode);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int getdents64(int fd, byte* buffer, int count);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial nint read(int fd, byte* buffer, nuint count);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial nint write(int fd, byte* buffer, nuint count);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial long lseek(int fd, long offset, int whence);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int rmdir(ReadOnlySpan<byte> pathname);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int fstat(int fd, out LinuxStat buf);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int lstat(ReadOnlySpan<byte> pathname, out LinuxStat buf);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int stat(ReadOnlySpan<byte> pathname, out LinuxStat buf);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial nint readlink(ReadOnlySpan<byte> pathname, Span<byte> buf, nuint bufsiz);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int utimensat(int dirfd, ReadOnlySpan<byte> pathname, ReadOnlySpan<Timespec> times, int flags);

    internal const int AT_FDCWD = -100;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Timespec {
        public long tv_sec;
        public long tv_nsec;
    }

    [LibraryImport("libc")]
    internal static partial nint sendfile(int outFd, int inFd, nint offset, nuint count);

    internal const int AT_REMOVEDIR = 0x200;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LinuxStat {
        public long st_dev;
        public long st_ino;
        public long st_nlink;
        public int st_mode;
        public int st_uid;
        public int st_gid;
        public int __pad1;
        public long st_rdev;
        public long st_size;
        public long st_blksize;
        public long st_blocks;
        public long st_atime;
        public long st_atime_nsec;
        public long st_mtime;
        public long st_mtime_nsec;
        public long st_ctime;
        public long st_ctime_nsec;
        public long __unused4;
        public long __unused5;
        public long __unused6;
    }
}
