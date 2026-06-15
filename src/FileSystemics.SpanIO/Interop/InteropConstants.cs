namespace FileSystemics.IO.Interop;

internal static class InteropConstants {
    // Unix open flags (Linux)
    internal const int O_RDONLY = 0;
    internal const int O_WRONLY = 1;
    internal const int O_RDWR = 2;
    internal const int O_CREAT = 0x40;
    internal const int O_EXCL = 0x80;
    internal const int O_TRUNC = 0x200;

    internal const int AT_FDCWD = -100;

    internal const int S_IFMT = 0xF000;
    internal const int S_IFDIR = 0x4000;
    internal const int S_IFREG = 0x8000;
    internal const int S_IFLNK = 0xA000;

    internal const int DT_DIR = 4;
    internal const int DT_REG = 8;

    internal const int R_OK = 4;
    internal const int W_OK = 2;
    internal const int X_OK = 1;
    internal const int F_OK = 0;

    // Windows
    internal const uint GENERIC_READ = 0x80000000;
    internal const uint GENERIC_WRITE = 0x40000000;
    internal const uint FILE_WRITE_ATTRIBUTES = 0x00000100;
    internal const uint FILE_SHARE_READ = 0x00000001;
    internal const uint FILE_SHARE_WRITE = 0x00000002;
    internal const uint FILE_SHARE_DELETE = 0x00000004;
    internal const uint OPEN_EXISTING = 3;
    internal const uint OPEN_ALWAYS = 4;
    internal const uint CREATE_ALWAYS = 2;
    internal const uint CREATE_NEW = 1;
    internal const uint TRUNCATE_EXISTING = 5;
    internal const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    internal const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
    internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    internal const uint FILE_FLAG_APPEND = 0x08000000;
    internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;
    internal const uint INVALID_FILE_ATTRIBUTES = unchecked((uint)-1);
    internal static readonly nint INVALID_HANDLE_VALUE = new(-1);
}
