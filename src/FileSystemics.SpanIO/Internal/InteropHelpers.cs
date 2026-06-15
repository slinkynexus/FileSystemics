using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FileSystemics.IO.Internal;

internal static class InteropHelpers {
    internal static void ThrowExceptionForLastError() {
        int error = Marshal.GetLastWin32Error();
        throw CreateExceptionForWin32Error(error);
    }

    internal static void ThrowExceptionForErrno(int errno) {
        throw CreateExceptionForErrno(errno);
    }

    internal static Exception CreateExceptionForWin32Error(int error) {
        return error switch {
            2 => new FileNotFoundException(new Win32Exception(error).Message),
            3 => new DirectoryNotFoundException(new Win32Exception(error).Message),
            5 => new UnauthorizedAccessException(new Win32Exception(error).Message),
            80 => new IOException(new Win32Exception(error).Message),
            183 => new IOException(new Win32Exception(error).Message),
            _ => new IOException(new Win32Exception(error).Message),
        };
    }

    internal static Exception CreateExceptionForErrno(int errno) {
        return errno switch {
            2 => new FileNotFoundException($"No such file or directory (errno {errno})"),
            13 => new UnauthorizedAccessException($"Permission denied (errno {errno})"),
            17 => new IOException($"File exists (errno {errno})"),
            20 => new IOException($"Not a directory (errno {errno})"),
            21 => new IOException($"Is a directory (errno {errno})"),
            39 => new IOException($"Directory not empty (errno {errno})"),
            _ => new IOException($"Native error (errno {errno})"),
        };
    }
}
