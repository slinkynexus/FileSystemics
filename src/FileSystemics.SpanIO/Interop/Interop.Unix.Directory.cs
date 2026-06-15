using System.Runtime.InteropServices;

namespace FileSystemics.IO.Interop;

internal static partial class InteropUnixDir {
    [LibraryImport("libc", SetLastError = true)]
    internal static partial nint opendir(ReadOnlySpan<byte> name);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial nint readdir(nint dirp);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int closedir(nint dirp);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct Dirent {
        public ulong d_ino;
        public ulong d_seekoff;
        public ushort d_reclen;
        public ushort d_namlen;
        public byte d_type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] d_name;
    }
}
