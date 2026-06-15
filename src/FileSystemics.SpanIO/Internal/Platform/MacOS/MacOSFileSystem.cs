using System.Runtime.InteropServices;
using FileSystemics.IO.Interop;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO.Internal;

internal sealed class MacOSFileSystem : IFileSystemPlatform<MacOSFileSystem> {
    private MacOSFileSystem() {
    }

    public static bool Exists(ReadOnlySpan<char> path) {
        return MacOSPathRules.WithNativePath(path, (_, utf8Path) =>
                InteropMacOs.lstat(utf8Path, out InteropMacOs.MacOsStat macStat) == 0 &&
                (macStat.st_mode & InteropConstants.S_IFMT) == InteropConstants.S_IFREG);
    }

    public static bool DirectoryExists(ReadOnlySpan<char> path) {
        return MacOSPathRules.WithNativePath(path, (_, utf8Path) =>
                InteropMacOs.lstat(utf8Path, out InteropMacOs.MacOsStat macStat) == 0 &&
                (macStat.st_mode & InteropConstants.S_IFMT) == InteropConstants.S_IFDIR);
    }

    public static SafeFileHandle OpenHandle(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, FileOptions options) {
        return MacOSPathRules.WithNativePath(path, (_, utf8Path) => PosixFileSystem.OpenHandleCore(utf8Path, mode, access, share));
    }

    public static SafeFileHandle OpenHandle(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName, FileMode mode, FileAccess access, FileShare share) {
        return MacOSPathRules.WithCombinedNativePath(directory, fileName, '/', (_, utf8Path) => PosixFileSystem.OpenHandleCore(utf8Path, mode, access, share));
    }

    public static void Delete(ReadOnlySpan<char> path) {
        MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                if (InteropUnix.unlink(utf8Path) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return 0;
            });
    }

    public static void Copy(ReadOnlySpan<char> source, ReadOnlySpan<char> destination, bool overwrite) {
        if (!Exists(source)) {
            throw new FileNotFoundException("Source file not found.", source.ToString());
        }

        if (!overwrite && Exists(destination)) {
            throw new IOException("Destination file already exists.");
        }

        using SafeFileHandle sourceHandle = OpenHandle(source, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None);
        using SafeFileHandle destHandle = OpenHandle(destination, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.None);

        PosixFileSystem.CopyByReadWrite(sourceHandle, destHandle);
    }

    public static void Move(ReadOnlySpan<char> source, ReadOnlySpan<char> destination) {
        MacOSPathRules.WithTwoNativePaths(
            source,
            destination,
            (_, _) => throw new PlatformNotSupportedException(),
            (sourceUtf8, destUtf8) => {
                if (InteropUnix.rename(sourceUtf8, destUtf8) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return 0;
            });
    }

    public static FileAttributes GetAttributes(ReadOnlySpan<char> path) {
        return MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                int mode = GetMode(utf8Path);
                return PosixFileSystem.ModeToFileAttributes(mode);
            });
    }

    public static void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) {
        MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                int mode = GetMode(utf8Path);
                int newMode = mode;
                if ((attributes & FileAttributes.ReadOnly) != 0) {
                    newMode &= ~0x1FF & ~0x80;
                    newMode |= 0x120;
                }
                else {
                    newMode |= 0x80;
                }

                if (InteropUnix.chmod(utf8Path, newMode) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return 0;
            });
    }

    public static bool TryGetLinkTarget(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        int byteCapacity = PlatformPathBuffer.Utf8Capacity(path);
        Span<byte> utf8Path = byteCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS * 4
            ? stackalloc byte[byteCapacity]
            : new byte[byteCapacity];
        PlatformPathBuffer.EncodeUtf8(path, utf8Path);

        Span<byte> buffer = stackalloc byte[PlatformPathBuffer.STACK_THRESHOLD_CHARS * 4];
        nint read = InteropUnix.readlink(utf8Path, buffer, (nuint)buffer.Length);
        if (read < 0) {
            return false;
        }

        int charCount = System.Text.Encoding.UTF8.GetCharCount(buffer[..(int)read]);
        if (charCount > destination.Length) {
            return false;
        }

        charsWritten = System.Text.Encoding.UTF8.GetChars(buffer[..(int)read], destination);
        return true;
    }

    public static void Encrypt(ReadOnlySpan<char> path) =>
        throw new PlatformNotSupportedException();

    public static void Decrypt(ReadOnlySpan<char> path) =>
        throw new PlatformNotSupportedException();

    public static DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) =>
        GetTimestamps(path).CreationTimeUtc;

    public static DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) =>
        GetTimestamps(path).LastAccessTimeUtc;

    public static DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) =>
        GetTimestamps(path).LastWriteTimeUtc;

    public static void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        SetTimestamps(path, creationTimeUtc: value, lastAccessTimeUtc: null, lastWriteTimeUtc: null);

    public static void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        SetTimestamps(path, creationTimeUtc: null, lastAccessTimeUtc: value, lastWriteTimeUtc: null);

    public static void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime value) =>
        SetTimestamps(path, creationTimeUtc: null, lastAccessTimeUtc: null, lastWriteTimeUtc: value);

    public static UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) {
        int mode = MacOSPathRules.WithNativePath(path, (_, utf8Path) => GetMode(utf8Path));
        return (UnixFileMode)(mode & 0xFFF);
    }

    public static void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) {
        MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                if (InteropUnix.chmod(utf8Path, (int)mode) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return 0;
            });
    }

    public static long GetLength(ReadOnlySpan<char> path) {
        return MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                if (InteropMacOs.lstat(utf8Path, out InteropMacOs.MacOsStat macStat) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return macStat.st_size;
            });
    }

    private static (DateTime CreationTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTimeUtc) GetTimestamps(
        ReadOnlySpan<char> path) =>
        MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                if (InteropMacOs.lstat(utf8Path, out InteropMacOs.MacOsStat macStat) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return (
                    DateTime.UnixEpoch.AddSeconds(macStat.st_birthtimespec_sec).AddTicks(macStat.st_birthtimespec_nsec / 100),
                    DateTime.UnixEpoch.AddSeconds(macStat.st_atimespec_sec).AddTicks(macStat.st_atimespec_nsec / 100),
                    DateTime.UnixEpoch.AddSeconds(macStat.st_mtimespec_sec).AddTicks(macStat.st_mtimespec_nsec / 100));
            });

    private static void SetTimestamps(
        ReadOnlySpan<char> path,
        DateTime? creationTimeUtc,
        DateTime? lastAccessTimeUtc,
        DateTime? lastWriteTimeUtc) {
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

        MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                Span<InteropUnix.Timespec> times = stackalloc InteropUnix.Timespec[2];
                times[0] = PosixFileSystem.ToTimespec(access);
                times[1] = PosixFileSystem.ToTimespec(write);
                if (InteropUnix.utimensat(InteropUnix.AT_FDCWD, utf8Path, times, 0) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return 0;
            });
    }

    private static int GetMode(ReadOnlySpan<byte> utf8Path) {
        if (InteropMacOs.lstat(utf8Path, out InteropMacOs.MacOsStat macStat) != 0) {
            InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
        }

        return macStat.st_mode;
    }

    public static void CreateDirectory(ReadOnlySpan<char> path) {
        bool alreadyExists = DirectoryExists(path);

        MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                if (InteropUnix.mkdir(utf8Path, 0x1FF) != 0) {
                    int errno = Marshal.GetLastWin32Error();
                    if (errno == 17 && alreadyExists) {
                        return 0;
                    }

                    InteropHelpers.ThrowExceptionForErrno(errno);
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

        MacOSPathRules.WithNativePath(path, (_, utf8Path) => {
                if (InteropUnix.unlink(utf8Path) == 0) {
                    return 0;
                }

                int errno = Marshal.GetLastWin32Error();
                if (InteropUnix.rmdir(utf8Path) != 0) {
                    InteropHelpers.ThrowExceptionForErrno(errno);
                }

                return 0;
            });
    }

    public static IFileSystemDirectoryEnumerator EnumerateDirectory(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind) =>
        new FileSystemEntryEnumerator(directory, searchPattern, kind);
}

