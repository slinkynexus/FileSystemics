using FileSystemics.IO.Interop;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.Internal;

internal sealed class WindowsFileSystem : IFileSystemPlatform<WindowsFileSystem> {
    private WindowsFileSystem() {
    }

    public static bool Exists(ReadOnlySpan<char> path) {
        return WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                uint attributes = InteropWindows.GetFileAttributesW(utf16Path);
                if (attributes == InteropConstants.INVALID_FILE_ATTRIBUTES) {
                    return false;
                }

                return (attributes & InteropConstants.FILE_ATTRIBUTE_DIRECTORY) == 0;
            });
    }

    public static bool DirectoryExists(ReadOnlySpan<char> path) {
        return WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                uint attributes = InteropWindows.GetFileAttributesW(utf16Path);
                if (attributes == InteropConstants.INVALID_FILE_ATTRIBUTES) {
                    return false;
                }

                return (attributes & InteropConstants.FILE_ATTRIBUTE_DIRECTORY) != 0;
            });
    }

    public static SafeFileHandle OpenHandle(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, FileOptions options) {
        return WindowsPathRules.WithNativePath(path, (utf16Path, _) => CreateHandle(utf16Path, mode, access, share, options));
    }

    public static SafeFileHandle OpenHandle(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName, FileMode mode, FileAccess access, FileShare share) {
        return WindowsPathRules.WithCombinedNativePath(
            directory,
            fileName,
            Path.DirectorySeparatorChar,
            (utf16Path, _) => CreateHandle(utf16Path, mode, access, share, FileOptions.None));
    }

    public static void Delete(ReadOnlySpan<char> path) {
        WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                if (!InteropWindows.DeleteFileW(utf16Path)) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            });
    }

    public static void Copy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite) {
        WindowsPathRules.WithTwoNativePaths(
            source,
            destination,
            (sourcePath, destPath) => {
                if (!InteropWindows.CopyFileW(sourcePath, destPath, bFailIfExists: !overwrite)) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            },
            (_, _) => throw new PlatformNotSupportedException());
    }

    public static void Move(ReadOnlySpan<char> source, ReadOnlySpan<char> destination) {
        WindowsPathRules.WithTwoNativePaths(
            source,
            destination,
            (sourcePath, destPath) => {
                if (!InteropWindows.MoveFileExW(sourcePath, destPath, InteropWindows.MOVEFILE_REPLACE_EXISTING)) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            },
            (_, _) => throw new PlatformNotSupportedException());
    }

    public static FileAttributes GetAttributes(ReadOnlySpan<char> path) {
        return WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                uint attributes = InteropWindows.GetFileAttributesW(utf16Path);
                if (attributes == InteropConstants.INVALID_FILE_ATTRIBUTES) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return (FileAttributes)attributes;
            });
    }

    public static void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) {
        WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                if (!InteropWindows.SetFileAttributesW(utf16Path, (uint)attributes)) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            });
    }

    public static DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) =>
        GetTimestamps(path).CreationTimeUtc;

    public static DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) =>
        GetTimestamps(path).LastAccessTimeUtc;

    public static DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) =>
        GetTimestamps(path).LastWriteTimeUtc;

    public static void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        SetTimestamp(path, creationTimeUtc: value, lastAccessTimeUtc: null, lastWriteTimeUtc: null);

    public static void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        SetTimestamp(path, creationTimeUtc: null, lastAccessTimeUtc: value, lastWriteTimeUtc: null);

    public static void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        SetTimestamp(path, creationTimeUtc: null, lastAccessTimeUtc: null, lastWriteTimeUtc: value);

    public static long GetLength(ReadOnlySpan<char> path) {
        using SafeFileHandle handle = OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None);
        using FileStream stream = new(handle, FileAccess.Read);
        return stream.Length;
    }

    public static void Encrypt(ReadOnlySpan<char> path) {
        WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                if (!InteropWindows.EncryptFileW(utf16Path)) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            });
    }

    public static void Decrypt(ReadOnlySpan<char> path) {
        WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                if (!InteropWindows.DecryptFileW(utf16Path, 0)) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            });
    }

    public static bool TryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        return false;
    }

    public static UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) =>
        throw new PlatformNotSupportedException();

    public static void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) =>
        throw new PlatformNotSupportedException();

    private static (DateTime CreationTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTimeUtc) GetTimestamps(
        ReadOnlySpan<char> path) =>
        WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
            InteropWindowsFind.Win32FindData data = default;
            nint handle = InteropWindowsFind.FindFirstFileW(utf16Path, ref data);
            if (handle == InteropConstants.INVALID_HANDLE_VALUE) {
                InteropHelpers.ThrowExceptionForLastError();
            }

            try {
                return (
                    FileTimeToUtc(data.ftCreationTime),
                    FileTimeToUtc(data.ftLastAccessTime),
                    FileTimeToUtc(data.ftLastWriteTime));
            }
            finally {
                InteropWindowsFind.FindClose(handle);
            }
        });

    private static void SetTimestamp(
        ReadOnlySpan<char> path,
        DateTime? creationTimeUtc,
        DateTime? lastAccessTimeUtc,
        DateTime? lastWriteTimeUtc) {
        using SafeFileHandle handle = OpenTimestampHandle(path);
        (DateTime creation, DateTime access, DateTime write) = GetTimestamps(path);
        if (creationTimeUtc is DateTime creationOverride) {
            creation = creationOverride;
        }

        if (lastAccessTimeUtc is DateTime accessOverride) {
            access = accessOverride;
        }

        if (lastWriteTimeUtc is DateTime writeOverride) {
            write = writeOverride;
        }

        unsafe {
            long creationFileTime = creation.ToFileTimeUtc();
            long accessFileTime = access.ToFileTimeUtc();
            long writeFileTime = write.ToFileTimeUtc();
            if (!InteropWindows.SetFileTime(
                    handle.DangerousGetHandle(),
                    (nint)(&creationFileTime),
                    (nint)(&accessFileTime),
                    (nint)(&writeFileTime))) {
                InteropHelpers.ThrowExceptionForLastError();
            }
        }
    }

    private static DateTime FileTimeToUtc(System.Runtime.InteropServices.ComTypes.FILETIME fileTime) {
        long ticks = ((long)(uint)fileTime.dwHighDateTime << 32) | (uint)fileTime.dwLowDateTime;
        return DateTime.FromFileTimeUtc(ticks);
    }

    private static SafeFileHandle OpenTimestampHandle(ReadOnlySpan<char> path) {
        return WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
            uint attributes = InteropWindows.GetFileAttributesW(utf16Path);
            if (attributes == InteropConstants.INVALID_FILE_ATTRIBUTES) {
                InteropHelpers.ThrowExceptionForLastError();
            }

            bool isDirectory = (attributes & InteropConstants.FILE_ATTRIBUTE_DIRECTORY) != 0;
            uint desiredAccess = isDirectory
                ? InteropConstants.FILE_WRITE_ATTRIBUTES
                : InteropConstants.GENERIC_READ | InteropConstants.GENERIC_WRITE;
            uint shareMode = InteropConstants.FILE_SHARE_READ |
                InteropConstants.FILE_SHARE_WRITE |
                InteropConstants.FILE_SHARE_DELETE;
            uint flags = isDirectory
                ? InteropConstants.FILE_FLAG_BACKUP_SEMANTICS
                : InteropConstants.FILE_ATTRIBUTE_NORMAL;

            nint handleValue = InteropWindows.CreateFileW(
                utf16Path,
                desiredAccess,
                shareMode,
                nint.Zero,
                InteropConstants.OPEN_EXISTING,
                flags,
                nint.Zero);

            if (handleValue == InteropConstants.INVALID_HANDLE_VALUE) {
                InteropHelpers.ThrowExceptionForLastError();
            }

            return new SafeFileHandle(handleValue, ownsHandle: true);
        });
    }

    private static SafeFileHandle CreateHandle(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, FileOptions options) {
        uint desiredAccess = access switch {
            FileAccess.Read => InteropConstants.GENERIC_READ,
            FileAccess.Write => InteropConstants.GENERIC_WRITE,
            FileAccess.ReadWrite => InteropConstants.GENERIC_READ | InteropConstants.GENERIC_WRITE,
            _ => 0,
        };

        uint shareMode = 0;
        if (share.HasFlag(FileShare.Read)) {
            shareMode |= InteropConstants.FILE_SHARE_READ;
        }

        if (share.HasFlag(FileShare.Write)) {
            shareMode |= InteropConstants.FILE_SHARE_WRITE;
        }

        if (share.HasFlag(FileShare.Delete)) {
            shareMode |= InteropConstants.FILE_SHARE_DELETE;
        }

        uint creationDisposition = mode switch {
            FileMode.CreateNew => InteropConstants.CREATE_NEW,
            FileMode.Create => InteropConstants.CREATE_ALWAYS,
            FileMode.Open => InteropConstants.OPEN_EXISTING,
            FileMode.OpenOrCreate => InteropConstants.OPEN_ALWAYS,
            FileMode.Truncate => InteropConstants.TRUNCATE_EXISTING,
            FileMode.Append => InteropConstants.OPEN_ALWAYS,
            _ => InteropConstants.OPEN_EXISTING,
        };

        uint flags = InteropConstants.FILE_ATTRIBUTE_NORMAL;
        if (mode == FileMode.Append) {
            flags |= InteropConstants.FILE_FLAG_APPEND;
        }

        if (options.HasFlag(FileOptions.WriteThrough)) {
            flags |= 0x80000000;
        }

        nint handleValue = InteropWindows.CreateFileW(
            path,
            desiredAccess,
            shareMode,
            nint.Zero,
            creationDisposition,
            flags,
            nint.Zero);

        if (handleValue == InteropConstants.INVALID_HANDLE_VALUE) {
            InteropHelpers.ThrowExceptionForLastError();
        }

        return new SafeFileHandle(handleValue, ownsHandle: true);
    }

    public static void CreateDirectory(ReadOnlySpan<char> path) {
        bool alreadyExists = DirectoryExists(path);

        WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                if (!InteropWindows.CreateDirectoryW(utf16Path, nint.Zero)) {
                    int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    if (error == 183 && alreadyExists) {
                        return 0;
                    }

                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            });
    }

    public static void DeleteDirectory(ReadOnlySpan<char> path, bool recursive) {
        if (recursive) {
            int capacity = DirectoryEntryPathReader.GetEntryPathBufferCapacity(path);
            Span<char> entryPath = capacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS
                ? stackalloc char[capacity]
                : GC.AllocateUninitializedArray<char>(capacity);
            Span<char> nameBuffer = stackalloc char[DirectoryEntryPathReader.MaxEntryNameChars];
            IFileSystemDirectoryEnumerator enumerator = EnumerateDirectory(path, "*".AsSpan(), DirectoryEntryKind.All);
            try {
                while (enumerator.MoveNext()) {
                    if (!enumerator.TryGetCurrentEntryName(nameBuffer, out int nameLength) ||
                        !SpanPath.TryJoin(path, nameBuffer[..nameLength], entryPath, out int entryLength)) {
                        continue;
                    }

                    ReadOnlySpan<char> entrySpan = entryPath[..entryLength];
                    if (DirectoryExists(entrySpan)) {
                        DeleteDirectory(entrySpan, recursive: true);
                    }
                    else {
                        Delete(entrySpan);
                    }
                }
            }
            finally {
                enumerator.Dispose();
            }
        }

        WindowsPathRules.WithNativePath(path, (utf16Path, _) => {
                if (!InteropWindows.RemoveDirectoryW(utf16Path)) {
                    InteropHelpers.ThrowExceptionForLastError();
                }

                return 0;
            });
    }

    public static IFileSystemDirectoryEnumerator EnumerateDirectory(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind) =>
        new WindowsDirectoryEnumerator(directory, searchPattern, kind);
}

