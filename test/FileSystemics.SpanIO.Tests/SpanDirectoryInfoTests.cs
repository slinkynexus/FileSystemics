using FileSystemics.IO;
using FileSystemics.IO.Internal;

namespace FileSystemics.SpanIO.Tests;

public class SpanDirectoryInfoTests {
    [Test]
    public async Task Exists() {
        using TempDirectoryScope temp = new();
        bool exists = new SpanDirectoryInfo(temp.Path.AsSpan()).Exists;

        await Assert.That(exists).IsTrue();
        await Assert.That(new DirectoryInfo(temp.Path).Exists).IsTrue();
    }

    [Test]
    public async Task Create() {
        using TempDirectoryScope temp = new();
        string nested = temp.Combine("new-dir");
        SpanDirectoryInfo info = new(nested.AsSpan());
        info.Create();
        bool exists = info.Exists;

        await Assert.That(exists).IsTrue();
    }

    [Test]
    public async Task TryCreateSubdirectory() {
        using TempDirectoryScope temp = new();
        SpanDirectoryInfo root = new(temp.Path.AsSpan());
        char[] destination = new char[temp.Path.Length + 16];
        bool success = root.TryCreateSubdirectory("child".AsSpan(), destination, out int charsWritten);
        string childPath = destination.AsSpan(0, charsWritten).ToString();
        bool childExists = new SpanDirectoryInfo(childPath.AsSpan()).Exists;

        await Assert.That(success).IsTrue();
        await Assert.That(childExists).IsTrue();
    }

    [Test]
    public async Task Delete() {
        using TempDirectoryScope temp = new();
        string child = temp.Combine("empty-child");
        SpanDirectory.CreateDirectory(child.AsSpan());
        SpanDirectoryInfo info = new(child.AsSpan());
        info.Delete();
        bool exists = info.Exists;

        await Assert.That(exists).IsFalse();
    }

    [Test]
    [Arguments(1, false)]
    [Arguments(40, false)]
    [Arguments(40, true)]
    public async Task EnumerateFiles(int fileCount, bool includeLongPath) {
        int minDirectoryPathLength = includeLongPath ? PlatformPathBuffer.STACK_THRESHOLD_CHARS + 1 : 0;
        using DirectoryListingFixture fixture = new(minDirectoryPathLength);
        DirectoryEnumerationTestHelpers.CreateNumberedTxtFiles(fixture, fileCount);

        List<string> fileNames = Enumerable.Range(0, fileCount).Select(static i => $"file{i}.txt").ToList();
        SpanDirectoryInfo info = new(fixture.ListPathSpan);
        char[] entryBuffer = new char[DirectoryEnumerationTestHelpers.GetMaxEntryPathLength(fixture.ListPath, fileNames)];
        SpanDirectoryEntryEnumerator enumerator = info.EnumerateFiles(entryBuffer, "*.txt".AsSpan());
        List<string> names = DirectoryEnumerationTestHelpers.CollectFileNames(enumerator);
        enumerator.Dispose();

        await DirectoryEnumerationTestHelpers.AssertEnumerateFileNamesMatchGetFiles(fixture, names, includeLongPath);
    }

    [Test]
    public async Task TryMoveTo() {
        using TempDirectoryScope temp = new();
        string sourceDir = temp.Combine("source");
        string destRoot = temp.Combine("dest-root");
        SpanDirectory.CreateDirectory(sourceDir.AsSpan());
        SpanDirectory.CreateDirectory(destRoot.AsSpan());
        SpanDirectoryInfo source = new(sourceDir.AsSpan());
        char[] destination = new char[destRoot.Length + 16];
        bool success = source.TryMoveTo(destRoot.AsSpan(), destination, out int charsWritten);
        string movedPath = destination.AsSpan(0, charsWritten).ToString();
        bool movedExists = new SpanDirectoryInfo(movedPath.AsSpan()).Exists;
        bool sourceExists = source.Exists;

        await Assert.That(success).IsTrue();
        await Assert.That(movedExists).IsTrue();
        await Assert.That(sourceExists).IsFalse();
    }

    [Test]
    public async Task Constructor_NullCharacter_ThrowsArgumentException() {
        SpanIOException? exception = null;
        try {
            SpanDirectoryInfo info = new("bad\0path".AsSpan());
            _ = info.Exists;
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }

    [Test]
    public async Task TryCreateSubdirectory_EmptyPath_ThrowsArgumentException() {
        using TempDirectoryScope temp = new();
        SpanDirectoryInfo root = new(temp.Path.AsSpan());
        char[] destination = new char[temp.Path.Length + 16];
        SpanIOException? exception = null;
        try {
            root.TryCreateSubdirectory(ReadOnlySpan<char>.Empty, destination, out _);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }

    [Test]
    public async Task TryCreateSubdirectory_RootedPath_ThrowsArgumentException() {
        using TempDirectoryScope temp = new();
        SpanDirectoryInfo root = new(temp.Path.AsSpan());
        char[] destination = new char[temp.Path.Length + 32];
        SpanIOException? exception = null;
        try {
            root.TryCreateSubdirectory("/rooted-child".AsSpan(), destination, out _);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }
}
