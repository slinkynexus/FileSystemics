using System.Runtime.InteropServices;

namespace FileSystemics.IO.Interop;

internal static partial class InteropUnixDrive {
    [StructLayout(LayoutKind.Sequential)]
    internal struct Statvfs {
        internal ulong f_bsize;
        internal ulong f_frsize;
        internal ulong f_blocks;
        internal ulong f_bfree;
        internal ulong f_bavail;
        internal ulong f_files;
        internal ulong f_ffree;
        internal ulong f_favail;
        internal ulong f_fsid;
        internal ulong f_flag;
        internal ulong f_namemax;
    }

    [DllImport("libc", SetLastError = true)]
    internal static extern int statvfs(nint path, out Statvfs buf);
}
