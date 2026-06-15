using System.Text;
using FileSystemics.Abstractions;
using Microsoft.Win32.SafeHandles;

namespace FileSystemics.IO;

/// <summary>
/// Default <see cref="ISpanFileSystem"/> implementation delegating to static SpanIO APIs.
/// </summary>
public sealed class SpanFileSystem : ISpanFileSystem {
    private SpanFileSystem() {
        DirectoryInfo = new SpanDirectoryInfoFactoryAdapter(this);
        DriveInfo = new SpanDriveInfoFactoryAdapter(this);
        FileInfo = new SpanFileInfoFactoryAdapter(this);
        FileStream = new SpanFileStreamFactoryAdapter(this);
        FileSystemWatcher = new SpanFileSystemWatcherFactoryAdapter(this);
        FileVersionInfo = new SpanFileVersionInfoFactoryAdapter(this);
    }

    /// <summary>Shared default instance backed by the active platform binding.</summary>
    public static ISpanFileSystem Default { get; } = new SpanFileSystem();

    /// <inheritdoc/>
    public ISpanDirectory Directory { get; } = new DirectoryFacade();

    /// <inheritdoc/>
    public ISpanDirectoryInfoFactory DirectoryInfo { get; }

    /// <inheritdoc/>
    public ISpanDriveInfoFactory DriveInfo { get; }

    /// <inheritdoc/>
    public ISpanFile File { get; } = new FileFacade();

    /// <inheritdoc/>
    public ISpanFileInfoFactory FileInfo { get; }

    /// <inheritdoc/>
    public ISpanFileStreamFactory FileStream { get; }

    /// <inheritdoc/>
    public ISpanFileSystemWatcherFactory FileSystemWatcher { get; }

    /// <inheritdoc/>
    public ISpanFileVersionInfoFactory FileVersionInfo { get; }

    /// <inheritdoc/>
    public ISpanPath Path { get; } = new PathFacade();

    private sealed class PathFacade : ISpanPath {
        public char DirectorySeparatorChar => SpanPath.DirectorySeparatorChar;

        public char AltDirectorySeparatorChar => SpanPath.AltDirectorySeparatorChar;

        public char VolumeSeparatorChar => SpanPath.VolumeSeparatorChar;

        public char PathSeparator => SpanPath.PathSeparator;

        public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path) => SpanPath.GetFileName(path);

        public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path) => SpanPath.GetDirectoryName(path);

        public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path) => SpanPath.GetExtension(path);

        public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path) =>
            SpanPath.GetFileNameWithoutExtension(path);

        public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path) => SpanPath.GetPathRoot(path);

        public bool HasExtension(ReadOnlySpan<char> path) => SpanPath.HasExtension(path);

        public bool IsPathRooted(ReadOnlySpan<char> path) => SpanPath.IsPathRooted(path);

        public bool IsPathFullyQualified(ReadOnlySpan<char> path) => SpanPath.IsPathFullyQualified(path);

        public bool EndsInDirectorySeparator(ReadOnlySpan<char> path) => SpanPath.EndsInDirectorySeparator(path);

        public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path) =>
            SpanPath.TrimEndingDirectorySeparator(path);

        public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten) =>
            SpanPath.TryJoin(path1, path2, destination, out charsWritten);

        public ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination) =>
            SpanPath.Join(path1, path2, destination);

        public bool TryJoin(
            ReadOnlySpan<char> path1,
            ReadOnlySpan<char> path2,
            ReadOnlySpan<char> path3,
            Span<char> destination,
            out int charsWritten) =>
            SpanPath.TryJoin(path1, path2, path3, destination, out charsWritten);

        public ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination) =>
            SpanPath.Join(path1, path2, path3, destination);

        public bool TryCombine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten) =>
            SpanPath.TryCombine(path1, path2, destination, out charsWritten);

        public ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination) =>
            SpanPath.Combine(path1, path2, destination);

        public bool TryCombine(
            ReadOnlySpan<char> path1,
            ReadOnlySpan<char> path2,
            ReadOnlySpan<char> path3,
            Span<char> destination,
            out int charsWritten) =>
            SpanPath.TryCombine(path1, path2, path3, destination, out charsWritten);

        public ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination) =>
            SpanPath.Combine(path1, path2, path3, destination);

        public bool TryChangeExtension(
            ReadOnlySpan<char> path,
            ReadOnlySpan<char> extension,
            Span<char> destination,
            out int charsWritten) =>
            SpanPath.TryChangeExtension(path, extension, destination, out charsWritten);

        public ReadOnlySpan<char> ChangeExtension(ReadOnlySpan<char> path, ReadOnlySpan<char> extension, Span<char> destination) =>
            SpanPath.ChangeExtension(path, extension, destination);

        public bool TryGetRelativePath(
            ReadOnlySpan<char> relativeTo,
            ReadOnlySpan<char> path,
            Span<char> destination,
            out int charsWritten) =>
            SpanPath.TryGetRelativePath(relativeTo, path, destination, out charsWritten);

        public ReadOnlySpan<char> GetRelativePath(ReadOnlySpan<char> relativeTo, ReadOnlySpan<char> path, Span<char> destination) =>
            SpanPath.GetRelativePath(relativeTo, path, destination);
    }

    private sealed class FileFacade : ISpanFile {
        public bool Exists(ReadOnlySpan<char> path) => SpanFile.Exists(path);

        public SafeFileHandle OpenHandle(
            ReadOnlySpan<char> path,
            FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.Read,
            FileShare share = FileShare.Read,
            FileOptions options = FileOptions.None) =>
            SpanFile.OpenHandle(path, mode, access, share, options);

        public SafeFileHandle OpenHandle(
            ReadOnlySpan<char> directory,
            ReadOnlySpan<char> fileName,
            FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.Read,
            FileShare share = FileShare.Read) =>
            SpanFile.OpenHandle(directory, fileName, mode, access, share);

        public FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share) =>
            SpanFile.Open(path, mode, access, share);

        public FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access) =>
            SpanFile.Open(path, mode, access);

        public FileStream Open(ReadOnlySpan<char> path, FileMode mode) =>
            SpanFile.Open(path, mode);

        public FileStream OpenRead(ReadOnlySpan<char> path) =>
            SpanFile.OpenRead(path);

        public FileStream OpenWrite(ReadOnlySpan<char> path) =>
            SpanFile.OpenWrite(path);

        public FileStream Create(ReadOnlySpan<char> path) =>
            SpanFile.Create(path);

        public StreamReader OpenText(ReadOnlySpan<char> path) =>
            SpanFile.OpenText(path);

        public StreamWriter CreateText(ReadOnlySpan<char> path) =>
            SpanFile.CreateText(path);

        public StreamWriter AppendText(ReadOnlySpan<char> path) =>
            SpanFile.AppendText(path);

        public void Delete(ReadOnlySpan<char> path) => SpanFile.Delete(path);

        public void Copy(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName, bool overwrite = false) =>
            SpanFile.Copy(sourceFileName, destFileName, overwrite);

        public void Move(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName) =>
            SpanFile.Move(sourceFileName, destFileName);

        public byte[] ReadAllBytes(ReadOnlySpan<char> path) => SpanFile.ReadAllBytes(path);

        public void WriteAllBytes(ReadOnlySpan<char> path, ReadOnlySpan<byte> bytes) =>
            SpanFile.WriteAllBytes(path, bytes);

        public string ReadAllText(ReadOnlySpan<char> path, Encoding? encoding = null) =>
            SpanFile.ReadAllText(path, encoding);

        public void WriteAllText(ReadOnlySpan<char> path, ReadOnlySpan<char> contents, Encoding? encoding = null) =>
            SpanFile.WriteAllText(path, contents, encoding);

        public FileAttributes GetAttributes(ReadOnlySpan<char> path) => SpanFile.GetAttributes(path);

        public void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) =>
            SpanFile.SetAttributes(path, attributes);

        public DateTime GetCreationTime(ReadOnlySpan<char> path) => SpanFile.GetCreationTime(path);

        public DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) => SpanFile.GetCreationTimeUtc(path);

        public void SetCreationTime(ReadOnlySpan<char> path, DateTime creationTime) =>
            SpanFile.SetCreationTime(path, creationTime);

        public void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime creationTimeUtc) =>
            SpanFile.SetCreationTimeUtc(path, creationTimeUtc);

        public DateTime GetLastAccessTime(ReadOnlySpan<char> path) => SpanFile.GetLastAccessTime(path);

        public DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) => SpanFile.GetLastAccessTimeUtc(path);

        public void SetLastAccessTime(ReadOnlySpan<char> path, DateTime lastAccessTime) =>
            SpanFile.SetLastAccessTime(path, lastAccessTime);

        public void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime lastAccessTimeUtc) =>
            SpanFile.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

        public DateTime GetLastWriteTime(ReadOnlySpan<char> path) => SpanFile.GetLastWriteTime(path);

        public DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) => SpanFile.GetLastWriteTimeUtc(path);

        public void SetLastWriteTime(ReadOnlySpan<char> path, DateTime lastWriteTime) =>
            SpanFile.SetLastWriteTime(path, lastWriteTime);

        public void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime lastWriteTimeUtc) =>
            SpanFile.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

        public UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) => SpanFile.GetUnixFileMode(path);

        public void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) =>
            SpanFile.SetUnixFileMode(path, mode);

        public void Replace(
            ReadOnlySpan<char> sourceFileName,
            ReadOnlySpan<char> destinationFileName,
            ReadOnlySpan<char> destinationBackupFileName,
            bool ignoreMetadataErrors = false) =>
            SpanFile.Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);

        public void Encrypt(ReadOnlySpan<char> path) => SpanFile.Encrypt(path);

        public void Decrypt(ReadOnlySpan<char> path) => SpanFile.Decrypt(path);
    }

    private sealed class DirectoryFacade : ISpanDirectory {
        public bool Exists(ReadOnlySpan<char> path) => SpanDirectory.Exists(path);

        public void CreateDirectory(ReadOnlySpan<char> path) => SpanDirectory.CreateDirectory(path);

        public void Delete(ReadOnlySpan<char> path, bool recursive = false) => SpanDirectory.Delete(path, recursive);

        public void Move(ReadOnlySpan<char> sourceDirName, ReadOnlySpan<char> destDirName) =>
            SpanDirectory.Move(sourceDirName, destDirName);

        public ReadOnlySpan<char> GetParent(ReadOnlySpan<char> path) => SpanDirectory.GetParent(path);

        public ReadOnlySpan<char> GetDirectoryRoot(ReadOnlySpan<char> path) => SpanDirectory.GetDirectoryRoot(path);

        public string GetCurrentDirectory() => SpanDirectory.GetCurrentDirectory();

        public void SetCurrentDirectory(ReadOnlySpan<char> path) => SpanDirectory.SetCurrentDirectory(path);

        public string[] GetLogicalDrives() => SpanDirectory.GetLogicalDrives();

        public FileAttributes GetAttributes(ReadOnlySpan<char> path) => SpanDirectory.GetAttributes(path);

        public void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) =>
            SpanDirectory.SetAttributes(path, attributes);

        public DateTime GetCreationTime(ReadOnlySpan<char> path) => SpanDirectory.GetCreationTime(path);

        public DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) => SpanDirectory.GetCreationTimeUtc(path);

        public void SetCreationTime(ReadOnlySpan<char> path, DateTime creationTime) =>
            SpanDirectory.SetCreationTime(path, creationTime);

        public void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime creationTimeUtc) =>
            SpanDirectory.SetCreationTimeUtc(path, creationTimeUtc);

        public DateTime GetLastAccessTime(ReadOnlySpan<char> path) => SpanDirectory.GetLastAccessTime(path);

        public DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) => SpanDirectory.GetLastAccessTimeUtc(path);

        public void SetLastAccessTime(ReadOnlySpan<char> path, DateTime lastAccessTime) =>
            SpanDirectory.SetLastAccessTime(path, lastAccessTime);

        public void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime lastAccessTimeUtc) =>
            SpanDirectory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

        public DateTime GetLastWriteTime(ReadOnlySpan<char> path) => SpanDirectory.GetLastWriteTime(path);

        public DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) => SpanDirectory.GetLastWriteTimeUtc(path);

        public void SetLastWriteTime(ReadOnlySpan<char> path, DateTime lastWriteTime) =>
            SpanDirectory.SetLastWriteTime(path, lastWriteTime);

        public void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime lastWriteTimeUtc) =>
            SpanDirectory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

        public UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) => SpanDirectory.GetUnixFileMode(path);

        public void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) =>
            SpanDirectory.SetUnixFileMode(path, mode);

        public int GetEntryPathBufferCapacity(ReadOnlySpan<char> directoryPath) =>
            SpanDirectory.GetEntryPathBufferCapacity(directoryPath);

        public SpanDirectoryEntryEnumerator EnumerateFiles(
            ReadOnlySpan<char> path,
            Span<char> buffer,
            ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.EnumerateFiles(path, buffer, searchPattern);

        public SpanDirectoryEntryEnumerator EnumerateDirectories(
            ReadOnlySpan<char> path,
            Span<char> buffer,
            ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.EnumerateDirectories(path, buffer, searchPattern);

        public SpanDirectoryEntryEnumerator EnumerateFileSystemEntries(
            ReadOnlySpan<char> path,
            Span<char> buffer,
            ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.EnumerateFileSystemEntries(path, buffer, searchPattern);

        public string[] GetFiles(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.GetFiles(path, buffer, searchPattern);

        public string[] GetFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.GetFiles(path, searchPattern);

        public string[] GetDirectories(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.GetDirectories(path, buffer, searchPattern);

        public string[] GetDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.GetDirectories(path, searchPattern);

        public string[] GetFileSystemEntries(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.GetFileSystemEntries(path, buffer, searchPattern);

        public string[] GetFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.GetFileSystemEntries(path, searchPattern);

        public IEnumerable<string> EnumerateFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.EnumerateFiles(path, searchPattern);

        public IEnumerable<string> EnumerateDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.EnumerateDirectories(path, searchPattern);

        public IEnumerable<string> EnumerateFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
            SpanDirectory.EnumerateFileSystemEntries(path, searchPattern);
    }
}
