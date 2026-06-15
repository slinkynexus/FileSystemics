using FileSystemics.Abstractions;

namespace FileSystemics.SpanIO.Platform.Tests;

using FileSystemics.IO;
using IOPath = System.IO.Path;

[NotInParallel("platform-binding")]
public partial class SpanFileSystemAdapterTests {
    [Test]
    public async Task Default_CreateDirectory_MatchesDirectSpanDirectory() {
        using TempDirectoryScope temp = new();
        ReadOnlySpan<char> nested = temp.Combine("a", "b", "c").AsSpan();

        SpanFileSystem.Default.Directory.CreateDirectory(nested);

        bool existsViaDirect = SpanDirectory.Exists(nested);
        await Assert.That(existsViaDirect).IsTrue();
        await Assert.That(System.IO.Directory.Exists(temp.Combine("a", "b", "c"))).IsTrue();
    }

    [Test]
    public async Task Default_FileReadWrite_MatchesDirectSpanFile() {
        using TempDirectoryScope temp = new();
        ReadOnlySpan<char> filePath = temp.Combine("data.txt").AsSpan();
        ReadOnlySpan<char> contents = "adapter-test".AsSpan();

        SpanFileSystem.Default.File.WriteAllText(filePath, contents);
        string viaAdapter = SpanFileSystem.Default.File.ReadAllText(filePath);
        string viaDirect = SpanFile.ReadAllText(filePath);

        bool existsViaAdapter = SpanFileSystem.Default.File.Exists(filePath);

        await Assert.That(viaAdapter).IsEqualTo("adapter-test");
        await Assert.That(viaDirect).IsEqualTo(viaAdapter);
        await Assert.That(existsViaAdapter).IsTrue();
    }

    [Test]
    public async Task Default_PathJoin_MatchesDirectSpanPath() {
        ReadOnlySpan<char> left = "root".AsSpan();
        ReadOnlySpan<char> right = "child".AsSpan();
        Span<char> adapterDestination = stackalloc char[64];
        Span<char> directDestination = stackalloc char[64];

        bool adapterSuccess = SpanFileSystem.Default.Path.TryJoin(left, right, adapterDestination, out int adapterLength);
        bool directSuccess = SpanPath.TryJoin(left, right, directDestination, out int directLength);
        string viaAdapter = adapterDestination[..adapterLength].ToString();
        string viaDirect = directDestination[..directLength].ToString();

        await Assert.That(adapterSuccess).IsTrue();
        await Assert.That(directSuccess).IsTrue();
        await Assert.That(viaAdapter).IsEqualTo(viaDirect);
    }

    [Test]
    public async Task Default_GetFiles_MatchesDirectSpanDirectory() {
        using TempDirectoryScope temp = new();
        temp.CreateEmptyFile("one.txt");
        temp.CreateEmptyFile("two.log");
        ReadOnlySpan<char> dir = temp.Path.AsSpan();

        string[] viaAdapter = SpanFileSystem.Default.Directory.GetFiles(dir, "*.txt");
        string[] viaDirect = SpanDirectory.GetFiles(dir, "*.txt");

        await Assert.That(viaAdapter).IsEquivalentTo(viaDirect);
    }

    [Test]
    public async Task Default_EnumerateFiles_MatchesDirectSpanDirectory() {
        using TempDirectoryScope temp = new();
        temp.CreateEmptyFile("one.txt");
        temp.CreateEmptyFile("two.txt");
        temp.CreateEmptyFile("three.log");
        ReadOnlySpan<char> dir = temp.Path.AsSpan();
        ReadOnlySpan<char> pattern = "*.txt".AsSpan();
        Span<char> buffer = stackalloc char[SpanFileSystem.Default.Directory.GetEntryPathBufferCapacity(dir)];

        List<string> viaAdapter = [];
        foreach (ReadOnlySpan<char> path in SpanFileSystem.Default.Directory.EnumerateFiles(dir, buffer, pattern)) {
            viaAdapter.Add(path.ToString());
        }

        List<string> viaDirect = [];
        foreach (ReadOnlySpan<char> path in SpanDirectory.EnumerateFiles(dir, buffer, pattern)) {
            viaDirect.Add(path.ToString());
        }

        await Assert.That(viaAdapter).IsEquivalentTo(viaDirect);
    }

    [Test]
    public async Task Default_DirectoryInfo_EnumerateFiles_MatchesDirectSpanDirectoryInfo() {
        using TempDirectoryScope temp = new();
        temp.CreateEmptyFile("one.txt");
        temp.CreateEmptyFile("two.log");
        ReadOnlySpan<char> dir = temp.Path.AsSpan();
        ReadOnlySpan<char> pattern = "*.txt".AsSpan();
        Span<char> buffer = stackalloc char[SpanDirectory.GetEntryPathBufferCapacity(dir)];

        List<string> viaAdapter = [];
        foreach (ReadOnlySpan<char> path in SpanFileSystem.Default.DirectoryInfo.New(dir).EnumerateFiles(buffer, pattern)) {
            viaAdapter.Add(path.ToString());
        }

        List<string> viaDirect = [];
        foreach (ReadOnlySpan<char> path in new SpanDirectoryInfo(dir).EnumerateFiles(buffer, pattern)) {
            viaDirect.Add(path.ToString());
        }

        await Assert.That(viaAdapter).IsEquivalentTo(viaDirect);
    }

    [Test]
    public async Task Default_DirectoryInfo_New_MatchesDirectSpanDirectoryInfo() {
        using TempDirectoryScope temp = new();
        ReadOnlySpan<char> path = temp.Path.AsSpan();

        ISpanDirectoryInfo viaAdapter = SpanFileSystem.Default.DirectoryInfo.New(path);
        SpanDirectoryInfo viaDirect = new(path);

        await Assert.That(viaAdapter.FullName.ToString()).IsEqualTo(viaDirect.FullName.ToString());
        await Assert.That(viaAdapter.FullName.ToString()).IsEqualTo(temp.Path);
    }

    [Test]
    public async Task Default_FileInfo_New_MatchesDirectSpanFileInfo() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateEmptyFile("info.txt");
        ReadOnlySpan<char> path = filePath.AsSpan();

        ISpanFileInfo viaAdapter = SpanFileSystem.Default.FileInfo.New(path);
        SpanFileInfo viaDirect = new(path);

        await Assert.That(viaAdapter.FullName.ToString()).IsEqualTo(viaDirect.FullName.ToString());
    }

    [Test]
    public async Task Default_DriveInfo_GetDrives_ReturnsAtLeastOneDrive() {
        ISpanDriveInfo[] viaAdapter = SpanFileSystem.Default.DriveInfo.GetDrives();
        DriveInfo[] viaDirect = DriveInfo.GetDrives();

        await Assert.That(viaAdapter.Length).IsGreaterThan(0);
        await Assert.That(viaAdapter.Length).IsEqualTo(viaDirect.Length);
    }

    [Test]
    public async Task Default_DriveInfo_New_MatchesDirectSpanDriveInfo() {
        ReadOnlySpan<char> driveName = OperatingSystem.IsWindows() ? @"C:\".AsSpan() : "/".AsSpan();

        ISpanDriveInfo viaAdapter = SpanFileSystem.Default.DriveInfo.New(driveName);
        SpanDriveInfo viaDirect = new(driveName);

        await Assert.That(viaAdapter.Name.ToString()).IsEqualTo(viaDirect.Name.ToString());
    }

    [Test]
    public async Task Default_FileStream_New_ReadsWrittenBytes() {
        using TempDirectoryScope temp = new();
        ReadOnlySpan<char> filePath = temp.Combine("stream.bin").AsSpan();
        byte[] payload = [1, 2, 3, 4];

        SpanFileSystem.Default.File.WriteAllBytes(filePath, payload);

        using Stream stream = SpanFileSystem.Default.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] buffer = new byte[payload.Length];

        int read = stream.Read(buffer, 0, buffer.Length);

        await Assert.That(read).IsEqualTo(payload.Length);
        await Assert.That(buffer).IsEquivalentTo(payload);
    }

    [Test]
    public async Task Default_FileSystemWatcher_New_DoesNotThrow() {
        using TempDirectoryScope temp = new();
        ReadOnlySpan<char> path = temp.Path.AsSpan();

        using ISpanFileSystemWatcher watcher = SpanFileSystem.Default.FileSystemWatcher.New(path);

        await Assert.That(watcher).IsNotNull();
    }

    [Test]
    public async Task Default_FileVersionInfo_GetVersionInfo_ReturnsRequestedPath() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateEmptyFile("version.txt");
        ReadOnlySpan<char> path = filePath.AsSpan();

        ISpanFileVersionInfo versionInfo = SpanFileSystem.Default.FileVersionInfo.GetVersionInfo(path);

        await Assert.That(versionInfo.FileName.ToString()).IsEqualTo(filePath);
    }

    [Test]
    public async Task Default_FileInfo_ExistsAndLength_MatchDirectSpanFileInfo() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateEmptyFile("metrics.bin");
        ReadOnlySpan<char> path = filePath.AsSpan();

        ISpanFileInfo viaAdapter = SpanFileSystem.Default.FileInfo.New(path);
        SpanFileInfo viaDirect = new(path);

        bool existsViaAdapter = viaAdapter.Exists;
        bool existsViaDirect = viaDirect.Exists;
        long lengthViaAdapter = viaAdapter.Length;
        long lengthViaDirect = viaDirect.Length;
        string nameViaAdapter = viaAdapter.Name.ToString();
        string nameViaDirect = viaDirect.Name.ToString();

        await Assert.That(existsViaAdapter).IsEqualTo(existsViaDirect);
        await Assert.That(lengthViaAdapter).IsEqualTo(lengthViaDirect);
        await Assert.That(nameViaAdapter).IsEqualTo(nameViaDirect);
    }

    [Test]
    public async Task Default_DirectoryInfo_GetFiles_MatchesDirectSpanDirectory() {
        using TempDirectoryScope temp = new();
        temp.CreateEmptyFile("listed.txt");
        temp.CreateEmptyFile("ignored.log");
        ReadOnlySpan<char> dir = temp.Path.AsSpan();

        string[] viaAdapter = SpanFileSystem.Default.DirectoryInfo.New(dir).GetFiles("*.txt");
        string[] viaDirect = SpanDirectory.GetFiles(dir, "*.txt");

        await Assert.That(viaAdapter).IsEquivalentTo(viaDirect);
    }

    [Test]
    public async Task Default_DirectoryInfo_CreateSubdirectory_CreatesNestedPath() {
        using TempDirectoryScope temp = new();
        ReadOnlySpan<char> root = temp.Path.AsSpan();

        ISpanDirectoryInfo nested = SpanFileSystem.Default.DirectoryInfo.New(root).CreateSubdirectory("a/b".AsSpan());

        await Assert.That(SpanDirectory.Exists(nested.FullName)).IsTrue();
        await Assert.That(nested.FullName.ToString()).IsEqualTo(IOPath.GetFullPath(temp.Combine("a", "b")));
    }

    [Test]
    public async Task Default_FactoryMembers_ExposeRootFileSystem() {
        ISpanFileSystem fileSystem = SpanFileSystem.Default;

        await Assert.That(fileSystem.DirectoryInfo.FileSystem).IsSameReferenceAs(fileSystem);
        await Assert.That(fileSystem.DriveInfo.FileSystem).IsSameReferenceAs(fileSystem);
        await Assert.That(fileSystem.FileInfo.FileSystem).IsSameReferenceAs(fileSystem);
        await Assert.That(fileSystem.FileStream.FileSystem).IsSameReferenceAs(fileSystem);
        await Assert.That(fileSystem.FileSystemWatcher.FileSystem).IsSameReferenceAs(fileSystem);
        await Assert.That(fileSystem.FileVersionInfo.FileSystem).IsSameReferenceAs(fileSystem);
    }

    [Test]
    public async Task ISpanFileSystem_CanBeSubstitutedWithMock() {
        ISpanFileSystem fileSystem = new StubSpanFileSystem();
        ReadOnlySpan<char> path = "/mock/path".AsSpan();

        fileSystem.Directory.CreateDirectory(path);

        await Assert.That(((StubSpanFileSystem)fileSystem).CreateDirectoryCalls).IsEqualTo(1);
    }

    private sealed class StubSpanFileSystem : ISpanFileSystem {
        private readonly StubSpanDirectory _directory = new();

        internal StubSpanFileSystem() {
            DirectoryInfo = new StubDirectoryInfoFactory(this);
            DriveInfo = new StubDriveInfoFactory(this);
            FileInfo = new StubFileInfoFactory(this);
            FileStream = new StubFileStreamFactory(this);
            FileSystemWatcher = new StubFileSystemWatcherFactory(this);
            FileVersionInfo = new StubFileVersionInfoFactory(this);
        }

        internal int CreateDirectoryCalls => _directory.CreateDirectoryCalls;

        public ISpanDirectory Directory => _directory;

        public ISpanDirectoryInfoFactory DirectoryInfo { get; }

        public ISpanDriveInfoFactory DriveInfo { get; }

        public ISpanFile File { get; } = new StubSpanFile();

        public ISpanFileInfoFactory FileInfo { get; }

        public ISpanFileStreamFactory FileStream { get; }

        public ISpanFileSystemWatcherFactory FileSystemWatcher { get; }

        public ISpanFileVersionInfoFactory FileVersionInfo { get; }

        public ISpanPath Path { get; } = new StubSpanPath();

        private class StubFactory(ISpanFileSystem fileSystem) : ISpanFileSystemEntity {
            public ISpanFileSystem FileSystem { get; } = fileSystem;
        }

        private sealed class StubDirectoryInfoFactory(ISpanFileSystem fileSystem)
            : StubFactory(fileSystem), ISpanDirectoryInfoFactory {
            public ISpanDirectoryInfo New(ReadOnlySpan<char> path) => throw new NotImplementedException();
            public ISpanDirectoryInfo? Wrap(DirectoryInfo? directoryInfo) => throw new NotImplementedException();
        }

        private sealed class StubDriveInfoFactory(ISpanFileSystem fileSystem)
            : StubFactory(fileSystem), ISpanDriveInfoFactory {
            public ISpanDriveInfo[] GetDrives() => throw new NotImplementedException();
            public ISpanDriveInfo New(ReadOnlySpan<char> driveName) => throw new NotImplementedException();
            public ISpanDriveInfo? Wrap(DriveInfo? driveInfo) => throw new NotImplementedException();
        }

        private sealed class StubFileInfoFactory(ISpanFileSystem fileSystem)
            : StubFactory(fileSystem), ISpanFileInfoFactory {
            public ISpanFileInfo New(ReadOnlySpan<char> fileName) => throw new NotImplementedException();
            public ISpanFileInfo? Wrap(FileInfo? fileInfo) => throw new NotImplementedException();
        }

        private sealed class StubFileStreamFactory(ISpanFileSystem fileSystem)
            : StubFactory(fileSystem), ISpanFileStreamFactory {
            public Stream New(Microsoft.Win32.SafeHandles.SafeFileHandle handle, FileAccess access) =>
                throw new NotImplementedException();
            public Stream New(Microsoft.Win32.SafeHandles.SafeFileHandle handle, FileAccess access, int bufferSize) =>
                throw new NotImplementedException();
            public Stream New(
                Microsoft.Win32.SafeHandles.SafeFileHandle handle,
                FileAccess access,
                int bufferSize,
                bool isAsync) => throw new NotImplementedException();
            public Stream New(ReadOnlySpan<char> path, FileMode mode) => throw new NotImplementedException();
            public Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access) =>
                throw new NotImplementedException();
            public Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share) =>
                throw new NotImplementedException();
            public Stream New(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, int bufferSize) =>
                throw new NotImplementedException();
            public Stream New(
                ReadOnlySpan<char> path,
                FileMode mode,
                FileAccess access,
                FileShare share,
                int bufferSize,
                bool useAsync) => throw new NotImplementedException();
            public Stream New(
                ReadOnlySpan<char> path,
                FileMode mode,
                FileAccess access,
                FileShare share,
                int bufferSize,
                FileOptions options) => throw new NotImplementedException();
            public Stream New(ReadOnlySpan<char> path, FileStreamOptions options) => throw new NotImplementedException();
            public Stream Wrap(FileStream fileStream) => throw new NotImplementedException();
        }

        private sealed class StubFileSystemWatcherFactory(ISpanFileSystem fileSystem)
            : StubFactory(fileSystem), ISpanFileSystemWatcherFactory {
            public ISpanFileSystemWatcher New() => throw new NotImplementedException();
            public ISpanFileSystemWatcher New(ReadOnlySpan<char> path) => throw new NotImplementedException();
            public ISpanFileSystemWatcher New(ReadOnlySpan<char> path, ReadOnlySpan<char> filter) =>
                throw new NotImplementedException();
            public ISpanFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher) =>
                throw new NotImplementedException();
        }

        private sealed class StubFileVersionInfoFactory(ISpanFileSystem fileSystem)
            : StubFactory(fileSystem), ISpanFileVersionInfoFactory {
            public ISpanFileVersionInfo GetVersionInfo(ReadOnlySpan<char> fileName) =>
                throw new NotImplementedException();
        }

        private sealed class StubSpanPath : ISpanPath {
            public char DirectorySeparatorChar => '/';
            public char AltDirectorySeparatorChar => '\\';
            public char VolumeSeparatorChar => ':';
            public char PathSeparator => ';';
            public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path) => path;
            public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path) => ReadOnlySpan<char>.Empty;
            public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path) => ReadOnlySpan<char>.Empty;
            public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path) => path;
            public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path) => ReadOnlySpan<char>.Empty;
            public bool HasExtension(ReadOnlySpan<char> path) => false;
            public bool IsPathRooted(ReadOnlySpan<char> path) => false;
            public bool IsPathFullyQualified(ReadOnlySpan<char> path) => false;
            public bool EndsInDirectorySeparator(ReadOnlySpan<char> path) => false;
            public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path) => path;
            public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten) {
                charsWritten = 0;
                return false;
            }
            public ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination) => destination;
            public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination, out int charsWritten) {
                charsWritten = 0;
                return false;
            }
            public ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination) => destination;
            public bool TryCombine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten) {
                charsWritten = 0;
                return false;
            }
            public ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination) => destination;
            public bool TryCombine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination, out int charsWritten) {
                charsWritten = 0;
                return false;
            }
            public ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination) => destination;
            public bool TryChangeExtension(ReadOnlySpan<char> path, ReadOnlySpan<char> extension, Span<char> destination, out int charsWritten) {
                charsWritten = 0;
                return false;
            }
            public ReadOnlySpan<char> ChangeExtension(ReadOnlySpan<char> path, ReadOnlySpan<char> extension, Span<char> destination) => destination;
            public bool TryGetRelativePath(ReadOnlySpan<char> relativeTo, ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) {
                charsWritten = 0;
                return false;
            }
            public ReadOnlySpan<char> GetRelativePath(ReadOnlySpan<char> relativeTo, ReadOnlySpan<char> path, Span<char> destination) => destination;
        }

        private sealed class StubSpanFile : ISpanFile {
            public bool Exists(ReadOnlySpan<char> path) => false;
            public Microsoft.Win32.SafeHandles.SafeFileHandle OpenHandle(ReadOnlySpan<char> path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read, FileOptions options = FileOptions.None) =>
                throw new NotSupportedException();
            public Microsoft.Win32.SafeHandles.SafeFileHandle OpenHandle(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read) =>
                throw new NotSupportedException();
            public FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share) =>
                throw new NotSupportedException();
            public FileStream Open(ReadOnlySpan<char> path, FileMode mode, FileAccess access) =>
                throw new NotSupportedException();
            public FileStream Open(ReadOnlySpan<char> path, FileMode mode) =>
                throw new NotSupportedException();
            public FileStream OpenRead(ReadOnlySpan<char> path) => throw new NotSupportedException();
            public FileStream OpenWrite(ReadOnlySpan<char> path) => throw new NotSupportedException();
            public FileStream Create(ReadOnlySpan<char> path) => throw new NotSupportedException();
            public StreamReader OpenText(ReadOnlySpan<char> path) => throw new NotSupportedException();
            public StreamWriter CreateText(ReadOnlySpan<char> path) => throw new NotSupportedException();
            public StreamWriter AppendText(ReadOnlySpan<char> path) => throw new NotSupportedException();
            public void Delete(ReadOnlySpan<char> path) { }
            public void Copy(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName, bool overwrite = false) { }
            public void Move(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destFileName) { }
            public byte[] ReadAllBytes(ReadOnlySpan<char> path) => [];
            public void WriteAllBytes(ReadOnlySpan<char> path, ReadOnlySpan<byte> bytes) { }
            public string ReadAllText(ReadOnlySpan<char> path, System.Text.Encoding? encoding = null) => string.Empty;
            public void WriteAllText(ReadOnlySpan<char> path, ReadOnlySpan<char> contents, System.Text.Encoding? encoding = null) { }
            public FileAttributes GetAttributes(ReadOnlySpan<char> path) => FileAttributes.Normal;
            public void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) { }
            public DateTime GetCreationTime(ReadOnlySpan<char> path) => default;
            public DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) => default;
            public void SetCreationTime(ReadOnlySpan<char> path, DateTime creationTime) { }
            public void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime creationTimeUtc) { }
            public DateTime GetLastAccessTime(ReadOnlySpan<char> path) => default;
            public DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) => default;
            public void SetLastAccessTime(ReadOnlySpan<char> path, DateTime lastAccessTime) { }
            public void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime lastAccessTimeUtc) { }
            public DateTime GetLastWriteTime(ReadOnlySpan<char> path) => default;
            public DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) => default;
            public void SetLastWriteTime(ReadOnlySpan<char> path, DateTime lastWriteTime) { }
            public void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime lastWriteTimeUtc) { }
            public UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) => default;
            public void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) { }
            public void Replace(ReadOnlySpan<char> sourceFileName, ReadOnlySpan<char> destinationFileName, ReadOnlySpan<char> destinationBackupFileName, bool ignoreMetadataErrors = false) { }
            public void Encrypt(ReadOnlySpan<char> path) { }
            public void Decrypt(ReadOnlySpan<char> path) { }
        }

        private sealed class StubSpanDirectory : ISpanDirectory {
            internal int CreateDirectoryCalls { get; private set; }

            public bool Exists(ReadOnlySpan<char> path) => false;

            public void CreateDirectory(ReadOnlySpan<char> path) => CreateDirectoryCalls++;

            public void Delete(ReadOnlySpan<char> path, bool recursive = false) { }

            public void Move(ReadOnlySpan<char> sourceDirName, ReadOnlySpan<char> destDirName) { }

            public ReadOnlySpan<char> GetParent(ReadOnlySpan<char> path) => ReadOnlySpan<char>.Empty;

            public ReadOnlySpan<char> GetDirectoryRoot(ReadOnlySpan<char> path) => ReadOnlySpan<char>.Empty;

            public string GetCurrentDirectory() => string.Empty;

            public void SetCurrentDirectory(ReadOnlySpan<char> path) { }

            public string[] GetLogicalDrives() => [];

            public FileAttributes GetAttributes(ReadOnlySpan<char> path) => FileAttributes.Normal;

            public void SetAttributes(ReadOnlySpan<char> path, FileAttributes attributes) { }

            public DateTime GetCreationTime(ReadOnlySpan<char> path) => default;

            public DateTime GetCreationTimeUtc(ReadOnlySpan<char> path) => default;

            public void SetCreationTime(ReadOnlySpan<char> path, DateTime creationTime) { }

            public void SetCreationTimeUtc(ReadOnlySpan<char> path, DateTime creationTimeUtc) { }

            public DateTime GetLastAccessTime(ReadOnlySpan<char> path) => default;

            public DateTime GetLastAccessTimeUtc(ReadOnlySpan<char> path) => default;

            public void SetLastAccessTime(ReadOnlySpan<char> path, DateTime lastAccessTime) { }

            public void SetLastAccessTimeUtc(ReadOnlySpan<char> path, DateTime lastAccessTimeUtc) { }

            public DateTime GetLastWriteTime(ReadOnlySpan<char> path) => default;

            public DateTime GetLastWriteTimeUtc(ReadOnlySpan<char> path) => default;

            public void SetLastWriteTime(ReadOnlySpan<char> path, DateTime lastWriteTime) { }

            public void SetLastWriteTimeUtc(ReadOnlySpan<char> path, DateTime lastWriteTimeUtc) { }

            public UnixFileMode GetUnixFileMode(ReadOnlySpan<char> path) => default;

            public void SetUnixFileMode(ReadOnlySpan<char> path, UnixFileMode mode) { }

            public int GetEntryPathBufferCapacity(ReadOnlySpan<char> directoryPath) => directoryPath.Length + 260;

            public SpanDirectoryEntryEnumerator EnumerateFiles(
                ReadOnlySpan<char> path,
                Span<char> buffer,
                ReadOnlySpan<char> searchPattern = default) => default;

            public SpanDirectoryEntryEnumerator EnumerateDirectories(
                ReadOnlySpan<char> path,
                Span<char> buffer,
                ReadOnlySpan<char> searchPattern = default) => default;

            public SpanDirectoryEntryEnumerator EnumerateFileSystemEntries(
                ReadOnlySpan<char> path,
                Span<char> buffer,
                ReadOnlySpan<char> searchPattern = default) => default;

            public string[] GetFiles(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default) => [];

            public string[] GetFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) => [];

            public string[] GetDirectories(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default) => [];

            public string[] GetDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) => [];

            public string[] GetFileSystemEntries(ReadOnlySpan<char> path, Span<char> buffer, ReadOnlySpan<char> searchPattern = default) => [];

            public string[] GetFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) => [];

            public IEnumerable<string> EnumerateFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
                Array.Empty<string>();

            public IEnumerable<string> EnumerateDirectories(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
                Array.Empty<string>();

            public IEnumerable<string> EnumerateFileSystemEntries(ReadOnlySpan<char> path, ReadOnlySpan<char> searchPattern = default) =>
                Array.Empty<string>();
        }
    }

    private sealed class TempDirectoryScope : IDisposable {
        internal TempDirectoryScope() {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"filesystemics-platform-{Guid.NewGuid():N}");
            System.IO.Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        internal string Combine(params string[] segments) => System.IO.Path.Combine([Path, .. segments]);

        internal string CreateEmptyFile(string relativePath) {
            string fullPath = Combine(relativePath);
            string? parent = System.IO.Path.GetDirectoryName(fullPath);
            if (parent is not null && parent.Length > 0) {
                System.IO.Directory.CreateDirectory(parent);
            }

            System.IO.File.WriteAllBytes(fullPath, []);
            return fullPath;
        }

        public void Dispose() {
            if (System.IO.Directory.Exists(Path)) {
                System.IO.Directory.Delete(Path, recursive: true);
            }
        }
    }
}
