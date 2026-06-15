using FileSystemics.IO.Internal;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace FileSystemics.IO;

/// <summary>
/// Span-based file APIs mirroring common <see cref="File"/> operations.
/// </summary>
public static class SpanFile {
    /// <summary>Determines whether the specified file exists.</summary>
    public static bool Exists(ReadOnlySpan<char> path) {
        if (PathArgumentValidation.ShouldExistenceCheckReturnFalse(path)) {
            return false;
        }

        return PlatformOps.Exists(path);
    }

    /// <summary>Opens a file handle for the specified path.</summary>
    public static SafeFileHandle OpenHandle(ReadOnlySpan<char> path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read, FileOptions options = FileOptions.None) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return PlatformOps.OpenHandle(path, mode, access, share, options);
    }

    /// <summary>Opens a file handle by combining a directory span and file name span.</summary>
    public static SafeFileHandle OpenHandle(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read) {
        if (!directory.IsEmpty) {
            PathArgumentValidation.ValidatePath(directory, nameof(directory));
        }

        PathArgumentValidation.ValidatePath(fileName, nameof(fileName));

        return PlatformOps.OpenHandle(directory, fileName, mode, access, share);
    }

    /// <summary>Opens a file at the specified path.</summary>
    public static FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share) {
        FileStream stream = new(OpenHandle(path, mode, access, share), access);
        if (mode == FileMode.Append && OperatingSystem.IsWindows() && stream.CanSeek) {
            stream.Position = stream.Length;
        }

        return stream;
    }

    /// <summary>Opens a file at the specified path.</summary>
    public static FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access) =>
        Open(path, mode, access, FileShare.None);

    /// <summary>Opens a file at the specified path.</summary>
    public static FileStream Open(ReadOnlySpan<char> path, FileMode mode) =>
        Open(path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);

    /// <summary>Opens an existing file for reading.</summary>
    public static FileStream OpenRead(ReadOnlySpan<char> path) =>
        Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    /// <summary>Opens an existing file or creates a new file for writing.</summary>
    public static FileStream OpenWrite(ReadOnlySpan<char> path) =>
        Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

    /// <summary>Creates or overwrites a file.</summary>
    public static FileStream Create(ReadOnlySpan<char> path) =>
        Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

    /// <summary>Opens an existing file for reading text.</summary>
    public static StreamReader OpenText(ReadOnlySpan<char> path) =>
        new(OpenRead(path));

    /// <summary>Creates or opens a file for writing text.</summary>
    public static StreamWriter CreateText(ReadOnlySpan<char> path) =>
        new(OpenWrite(path));

    /// <summary>Creates a text writer that appends to a file.</summary>
    public static StreamWriter AppendText(ReadOnlySpan<char> path) =>
        new(Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));

    /// <summary>Opens a file by combining a directory span and file name span.</summary>
    public static FileStream Open(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName, FileMode mode, FileAccess access, FileShare share) =>
        new(OpenHandle(directory, fileName, mode, access, share), access);

    /// <summary>Deletes the specified file.</summary>
    public static void Delete(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.Delete(path);
    }

    /// <summary>Copies an existing file.</summary>
    public static void Copy(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName, bool overwrite = false) {
        PathArgumentValidation.ValidatePath(sourceFileName, nameof(sourceFileName));
        PathArgumentValidation.ValidatePath(destFileName, nameof(destFileName));

        PlatformOps.Copy(sourceFileName, destFileName, overwrite);
    }

    /// <summary>Moves a file.</summary>
    public static void Move(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName) {
        PathArgumentValidation.ValidatePath(sourceFileName, nameof(sourceFileName));
        PathArgumentValidation.ValidatePath(destFileName, nameof(destFileName));

        PlatformOps.Move(sourceFileName, destFileName);
    }

    /// <summary>Reads all bytes from the file.</summary>
    public static byte[] ReadAllBytes(ReadOnlySpan<char> path) {
        using SafeFileHandle handle = OpenHandle(path);
        long length = PlatformOps.GetLength(path);

        if (length == 0) {
            return [];
        }

        if (length > int.MaxValue) {
            throw new IOException("The file is too long.");
        }

        byte[] buffer = new byte[(int)length];
        RandomAccess.Read(handle, buffer, 0);
        return buffer;
    }

    /// <summary>Writes all bytes to the file.</summary>
    public static void WriteAllBytes(ReadOnlySpan<char> path, ReadOnlySpan<byte> bytes) {
        using SafeFileHandle handle = OpenHandle(path, FileMode.Create, FileAccess.Write, FileShare.None);
        RandomAccess.Write(handle, bytes, 0);
    }

    /// <summary>Reads all text from the file.</summary>
    public static string ReadAllText(ReadOnlySpan<char> path, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        byte[] bytes = ReadAllBytes(path);
        return encoding.GetString(bytes);
    }

    /// <summary>Writes all text to the file.</summary>
    public static void WriteAllText(ReadOnlySpan<char> path, ReadOnlySpan<char> contents, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        if (contents.IsEmpty) {
            WriteAllBytes(path, ReadOnlySpan<byte>.Empty);
            return;
        }

        byte[] buffer = new byte[encoding.GetByteCount(contents)];
        encoding.GetBytes(contents, buffer);
        WriteAllBytes(path, buffer);
    }

    /// <summary>Gets the attributes of the file.</summary>
    public static FileAttributes GetAttributes(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return PlatformOps.GetAttributes(path);
    }

    /// <summary>Sets the attributes of the file.</summary>
    public static void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.SetAttributes(path, attributes);
    }

    /// <summary>Gets local creation time.</summary>
    public static DateTime GetCreationTime(ReadOnlySpan<char> path) =>
        GetCreationTimeUtc(path).ToLocalTime();

    /// <summary>Gets UTC creation time.</summary>
    public static DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return PlatformOps.GetCreationTimeUtc(path);
    }

    /// <summary>Sets local creation time.</summary>
    public static void SetCreationTime(ReadOnlySpan<char> path, DateTime creationTime) =>
        SetCreationTimeUtc(path, creationTime.ToUniversalTime());

    /// <summary>Sets UTC creation time.</summary>
    public static void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime creationTimeUtc) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.SetCreationTimeUtc(path, creationTimeUtc);
    }

    /// <summary>Gets local last access time.</summary>
    public static DateTime GetLastAccessTime(ReadOnlySpan<char> path) =>
        GetLastAccessTimeUtc(path).ToLocalTime();

    /// <summary>Gets UTC last access time.</summary>
    public static DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return PlatformOps.GetLastAccessTimeUtc(path);
    }

    /// <summary>Sets local last access time.</summary>
    public static void SetLastAccessTime(ReadOnlySpan<char> path, DateTime lastAccessTime) =>
        SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());

    /// <summary>Sets UTC last access time.</summary>
    public static void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime lastAccessTimeUtc) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
    }

    /// <summary>Gets local last write time.</summary>
    public static DateTime GetLastWriteTime(ReadOnlySpan<char> path) =>
        GetLastWriteTimeUtc(path).ToLocalTime();

    /// <summary>Gets UTC last write time.</summary>
    public static DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return PlatformOps.GetLastWriteTimeUtc(path);
    }

    /// <summary>Sets local last write time.</summary>
    public static void SetLastWriteTime(ReadOnlySpan<char> path, DateTime lastWriteTime) =>
        SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());

    /// <summary>Sets UTC last write time.</summary>
    public static void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime lastWriteTimeUtc) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
    }

    /// <summary>Gets Unix file mode flags.</summary>
    public static UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return PlatformOps.GetUnixFileMode(path);
    }

    /// <summary>Sets Unix file mode flags.</summary>
    public static void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.SetUnixFileMode(path, mode);
    }

    /// <summary>Replaces the contents of a file with the contents of another file.</summary>
    public static void Replace(
        ReadOnlySpan<char> sourceFileName,
        ReadOnlySpan<char> destinationFileName,
        ReadOnlySpan<char> destinationBackupFileName,
        bool ignoreMetadataErrors = false) {
        PathArgumentValidation.ValidatePath(sourceFileName, nameof(sourceFileName));
        PathArgumentValidation.ValidatePath(destinationFileName, nameof(destinationFileName));
        PathArgumentValidation.ValidatePath(destinationBackupFileName, nameof(destinationBackupFileName));
        _ = ignoreMetadataErrors;

        if (Exists(destinationFileName)) {
            if (Exists(destinationBackupFileName)) {
                Delete(destinationBackupFileName);
            }

            Move(destinationFileName, destinationBackupFileName);
        }

        Move(sourceFileName, destinationFileName);
    }

    /// <summary>Encrypts a file.</summary>
    public static void Encrypt(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.Encrypt(path);
    }

    /// <summary>Decrypts a file.</summary>
    public static void Decrypt(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.Decrypt(path);
    }
}
