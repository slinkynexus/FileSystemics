using System.Runtime.InteropServices;

namespace FileSystemics.IO.Interop;

internal static partial class InteropMacOs {
    [LibraryImport("libc", SetLastError = true)]
    internal static partial int lstat(ReadOnlySpan<byte> pathname, out MacOsStat buf);

    [DllImport("libc", SetLastError = true)]
    internal static extern int statfs(nint path, out Statfs buf);

    private const int MntNowait = 0x01;

    [DllImport("libc", SetLastError = true)]
    internal static extern int getmntinfo(out nint mntbuf, int flags);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Statfs {
        public uint f_bsize;
        public int f_iosize;
        public ulong f_blocks;
        public ulong f_bfree;
        public ulong f_bavail;
        public ulong f_files;
        public ulong f_ffree;
        public int f_fsid_val0;
        public int f_fsid_val1;
        public uint f_owner;
        public uint f_type;
        public uint f_flags;
        public uint f_fssubtype;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string f_fstypename;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string f_mntonname;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string f_mntfromname;

        public uint f_flags_ext;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public uint[] f_reserved;
    }

    internal static int GetMountEntries(out nint buffer) => getmntinfo(out buffer, MntNowait);

    internal static int StatfsSize => Marshal.SizeOf<Statfs>();

    [StructLayout(LayoutKind.Sequential)]
    internal struct MacOsStat {
        public int st_dev;
        public ushort st_mode;
        public ushort st_nlink;
        public ulong st_ino;
        public uint st_uid;
        public uint st_gid;
        public int st_rdev;
        public long st_atimespec_sec;
        public long st_atimespec_nsec;
        public long st_mtimespec_sec;
        public long st_mtimespec_nsec;
        public long st_ctimespec_sec;
        public long st_ctimespec_nsec;
        public long st_birthtimespec_sec;
        public long st_birthtimespec_nsec;
        public long st_size;
        public long st_blocks;
        public int st_blksize;
        public uint st_flags;
        public uint st_gen;
        public int st_lspare;
        public long st_qspare0;
        public long st_qspare1;
    }
}
