using System.Runtime.InteropServices;

namespace FileSystemics.IO.Interop;

internal static partial class InteropWindows {
    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint CreateFileW(
        ReadOnlySpan<char> lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        nint lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        nint hTemplateFile);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeleteFileW(ReadOnlySpan<char> lpFileName);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RemoveDirectoryW(ReadOnlySpan<char> lpPathName);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CreateDirectoryW(ReadOnlySpan<char> lpPathName, nint lpSecurityAttributes);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CopyFileW(
        ReadOnlySpan<char> lpExistingFileName,
        ReadOnlySpan<char> lpNewFileName,
        [MarshalAs(UnmanagedType.Bool)] bool bFailIfExists);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool MoveFileExW(
        ReadOnlySpan<char> lpExistingFileName,
        ReadOnlySpan<char> lpNewFileName,
        uint dwFlags);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial uint GetFileAttributesW(ReadOnlySpan<char> lpFileName);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetFileAttributesW(ReadOnlySpan<char> lpFileName, uint dwFileAttributes);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetFileTime(
        nint hFile,
        nint lpCreationTime,
        nint lpLastAccessTime,
        nint lpLastWriteTime);

    [LibraryImport("advapi32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EncryptFileW(ReadOnlySpan<char> lpFileName);

    [LibraryImport("advapi32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DecryptFileW(ReadOnlySpan<char> lpFileName, uint dwReserved);

    internal const uint MOVEFILE_REPLACE_EXISTING = 0x00000001;
}

/// <summary>
/// Classic P/Invoke for directory enumeration; <see cref="LibraryImportAttribute"/> cannot marshal <see cref="Win32FindData"/>.
/// </summary>
internal static class InteropWindowsFind {
    /// <summary>
    /// Opens a directory search on a null-terminated UTF-16 path without allocating a managed <see cref="string"/>.
    /// </summary>
    internal static unsafe nint FindFirstFileW(ReadOnlySpan<char> nullTerminatedUtf16Path, ref Win32FindData lpFindFileData) {
        fixed (char* pathPtr = nullTerminatedUtf16Path) {
            return FindFirstFileWNative(pathPtr, ref lpFindFileData);
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "FindFirstFileW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    private static extern unsafe nint FindFirstFileWNative(char* lpFileName, ref Win32FindData lpFindFileData);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool FindNextFileW(nint hFindFile, ref Win32FindData lpFindFileData);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool FindClose(nint hFindFile);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct Win32FindData {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }
}
