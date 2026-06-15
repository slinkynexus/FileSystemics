using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

/// <summary>
/// Span-based directory APIs mirroring common <see cref="Directory"/> operations.
/// </summary>
public static class SpanDirectory {
    /// <summary>Determines whether the specified directory exists.</summary>
    public static bool Exists(ReadOnlySpan<char> path) {
        if (PathArgumentValidation.ShouldExistenceCheckReturnFalse(path)) {
            return false;
        }

        return PlatformOps.DirectoryExists(path);
    }

    /// <summary>Creates all directories in the specified path.</summary>
    public static void CreateDirectory(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        if (Exists(path)) {
            return;
        }

        ReadOnlySpan<char> parent = SpanPath.GetDirectoryName(path);
        if (!parent.IsEmpty && !Exists(parent)) {
            CreateDirectory(parent);
        }

        PlatformOps.CreateDirectory(path);
    }

    /// <summary>Deletes the specified directory.</summary>
    public static void Delete(ReadOnlySpan<char> path, bool recursive = false) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        PlatformOps.DeleteDirectory(path, recursive);
    }

    /// <summary>Moves a directory to a new location.</summary>
    public static void Move(ReadOnlySpan<char> sourceDirName, ReadOnlySpan<char> destDirName) {
        PathArgumentValidation.ValidatePath(sourceDirName, nameof(sourceDirName));
        PathArgumentValidation.ValidatePath(destDirName, nameof(destDirName));

        PlatformOps.Move(sourceDirName, destDirName);
    }

    /// <summary>Gets the parent directory of the specified path.</summary>
    public static ReadOnlySpan<char> GetParent(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return SpanPath.GetDirectoryName(path);
    }

    /// <summary>Gets the root directory of the specified path.</summary>
    public static ReadOnlySpan<char> GetDirectoryRoot(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return SpanPath.GetPathRoot(path);
    }

    /// <summary>Gets the current working directory.</summary>
    public static string GetCurrentDirectory() => Environment.CurrentDirectory;

    /// <summary>Sets the current working directory.</summary>
    public static void SetCurrentDirectory(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        Environment.CurrentDirectory = path.ToString();
    }

    /// <summary>Gets the names of logical drives on the computer.</summary>
    public static string[] GetLogicalDrives() {
        List<string>? drives = null;
        Span<char> nameBuffer = stackalloc char[PlatformPathBuffer.STACK_THRESHOLD_CHARS];
        foreach (ReadOnlySpan<char> drive in SpanDriveInfo.EnumerateDrives(nameBuffer)) {
            (drives ??= []).Add(drive.ToString());
        }

        return drives?.ToArray() ?? Array.Empty<string>();
    }

    /// <summary>Gets the attributes of the directory.</summary>
    public static FileAttributes GetAttributes(ReadOnlySpan<char> path) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return PlatformOps.GetAttributes(path);
    }

    /// <summary>Sets the attributes of the directory.</summary>
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

    /// <summary>
    /// Returns a recommended <c>char</c> buffer capacity for enumerating entry paths under <paramref name="directoryPath"/>.
    /// </summary>
    public static int GetEntryPathBufferCapacity(ReadOnlySpan<char> directoryPath) =>
        DirectoryEntryPathReader.GetEntryPathBufferCapacity(directoryPath);

    /// <summary>Enumerates files into <paramref name="buffer"/>.</summary>
    public static SpanDirectoryEntryEnumerator EnumerateFiles(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return new SpanDirectoryEntryEnumerator(path, searchPattern, DirectoryEntryKind.Files, buffer);
    }

    /// <summary>Enumerates subdirectories into <paramref name="buffer"/>.</summary>
    public static SpanDirectoryEntryEnumerator EnumerateDirectories(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return new SpanDirectoryEntryEnumerator(path, searchPattern, DirectoryEntryKind.Directories, buffer);
    }

    /// <summary>Enumerates files and subdirectories into <paramref name="buffer"/>.</summary>
    public static SpanDirectoryEntryEnumerator EnumerateFileSystemEntries(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        return new SpanDirectoryEntryEnumerator(path, searchPattern, DirectoryEntryKind.All, buffer);
    }

    /// <summary>Returns file paths using <paramref name="buffer"/> for enumeration.</summary>
    public static string[] GetFiles(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        CollectEntryPaths(path, buffer, searchPattern, DirectoryEntryKind.Files);

    /// <summary>Returns subdirectory paths using <paramref name="buffer"/> for enumeration.</summary>
    public static string[] GetDirectories(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        CollectEntryPaths(path, buffer, searchPattern, DirectoryEntryKind.Directories);

    /// <summary>Returns file and subdirectory paths using <paramref name="buffer"/> for enumeration.</summary>
    public static string[] GetFileSystemEntries(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern = default) =>
        CollectEntryPaths(path, buffer, searchPattern, DirectoryEntryKind.All);

    /// <summary>Returns the names of files in the specified directory.</summary>
    public static string[] GetFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) {
        char[] buffer = GC.AllocateUninitializedArray<char>(GetEntryPathBufferCapacity(path));
        return GetFiles(path, buffer, searchPattern);
    }

    /// <summary>Returns subdirectory names in the specified directory.</summary>
    public static string[] GetDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) {
        char[] buffer = GC.AllocateUninitializedArray<char>(GetEntryPathBufferCapacity(path));
        return GetDirectories(path, buffer, searchPattern);
    }

    /// <summary>Returns file and subdirectory names in the specified directory.</summary>
    public static string[] GetFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) {
        char[] buffer = GC.AllocateUninitializedArray<char>(GetEntryPathBufferCapacity(path));
        return GetFileSystemEntries(path, buffer, searchPattern);
    }

    /// <summary>Enumerates file names in the specified directory.</summary>
    public static IEnumerable<string> EnumerateFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
        GetFiles(path, searchPattern);

    /// <summary>Enumerates subdirectory names in the specified directory.</summary>
    public static IEnumerable<string> EnumerateDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
        GetDirectories(path, searchPattern);

    /// <summary>Enumerates file and subdirectory names in the specified directory.</summary>
    public static IEnumerable<string> EnumerateFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
        GetFileSystemEntries(path, searchPattern);

    private static string[] CollectEntryPaths(
        ReadOnlySpan<char> path,
        Span<char> buffer,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind) {
        PathArgumentValidation.ValidatePath(path, nameof(path));

        List<string>? results = null;
        using SpanDirectoryEntryEnumerator enumerator = kind switch {
            DirectoryEntryKind.Files => EnumerateFiles(path, buffer, searchPattern),
            DirectoryEntryKind.Directories => EnumerateDirectories(path, buffer, searchPattern),
            _ => EnumerateFileSystemEntries(path, buffer, searchPattern),
        };

        while (enumerator.MoveNext()) {
            (results ??= []).Add(enumerator.Current.ToString());
        }

        return results?.ToArray() ?? Array.Empty<string>();
    }
}
