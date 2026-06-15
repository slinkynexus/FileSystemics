using FileSystemics.IO;
using FileSystemics.IO.Internal;
using IOPath = System.IO.Path;

namespace FileSystemics.SpanIO.Tests;

public class SpanDirectoryTests {
    [Test]
    public async Task Exists() {
        await Assert.That(SpanDirectory.Exists(ReadOnlySpan<char>.Empty)).IsFalse();

        using TempDirectoryScope temp = new();
        string missing = temp.Combine("missing-dir");
        await Assert.That(SpanDirectory.Exists(missing.AsSpan())).IsFalse();

        temp.CreateFile("file.txt", "x".AsSpan());
        await Assert.That(SpanDirectory.Exists(temp.Combine("file.txt").AsSpan())).IsFalse();
        await Assert.That(SpanDirectory.Exists(temp.Path.AsSpan())).IsTrue();
    }

    [Test]
    public async Task CreateDirectory() {
        using TempDirectoryScope temp = new();
        string nested = temp.Combine("a", "b", "c");

        SpanDirectory.CreateDirectory(nested.AsSpan());
        await Assert.That(SpanDirectory.Exists(nested.AsSpan())).IsTrue();

        SpanDirectory.CreateDirectory(temp.Path.AsSpan());
        await Assert.That(SpanDirectory.Exists(temp.Path.AsSpan())).IsTrue();

        SpanIOException? emptyPath = null;
        try {
            SpanDirectory.CreateDirectory(ReadOnlySpan<char>.Empty);
        }
        catch (SpanIOException ex) {
            emptyPath = ex;
        }

        await Assert.That(emptyPath).IsNotNull();
    }

    [Test]
    public async Task Delete() {
        using TempDirectoryScope temp = new();
        string emptyChild = temp.Combine("empty-child");
        SpanDirectory.CreateDirectory(emptyChild.AsSpan());
        SpanDirectory.Delete(emptyChild.AsSpan(), recursive: false);
        await Assert.That(SpanDirectory.Exists(emptyChild.AsSpan())).IsFalse();

        using TempDirectoryScope nonEmptyDir = new();
        nonEmptyDir.CreateFile("child.txt", "x".AsSpan());
        Exception? nonEmptyException = null;
        try {
            SpanDirectory.Delete(nonEmptyDir.Path.AsSpan(), recursive: false);
        }
        catch (Exception ex) {
            nonEmptyException = ex;
        }

        await Assert.That(nonEmptyException).IsNotNull();

        using TempDirectoryScope recursive = new();
        SpanDirectory.CreateDirectory(recursive.Combine("a", "b").AsSpan());
        recursive.CreateFile("a/b/c.txt", "x".AsSpan());
        SpanDirectory.Delete(recursive.Path.AsSpan(), recursive: true);
        await Assert.That(SpanDirectory.Exists(recursive.Path.AsSpan())).IsFalse();

        SpanIOException? emptyPath = null;
        try {
            SpanDirectory.Delete(ReadOnlySpan<char>.Empty);
        }
        catch (SpanIOException ex) {
            emptyPath = ex;
        }

        await Assert.That(emptyPath).IsNotNull();
    }

    [Test]
    public async Task GetFiles() {
        using TempDirectoryScope temp = new();
        temp.CreateFile("a.txt", "a".AsSpan());
        temp.CreateFile("b.log", "b".AsSpan());
        temp.CreateFile("sub/nested.txt", "n".AsSpan());
        temp.CreateFile("alpha.txt", "a".AsSpan());
        temp.CreateFile("beta.txt", "b".AsSpan());
        temp.CreateFile("report.old", "a".AsSpan());
        temp.CreateFile("report.new", "b".AsSpan());
        temp.CreateFile("notes.txt", "c".AsSpan());
        temp.CreateFile("exact.txt", "a".AsSpan());
        temp.CreateFile("exact.txtx", "b".AsSpan());
        temp.CreateFile("one.bin", "1".AsSpan());
        temp.CreateFile("two.bin", "2".AsSpan());
        string itemPath = temp.CreateFile("item.txt", "x".AsSpan());

        string[] wildcard = SpanDirectory.GetFiles(temp.Path.AsSpan(), "*".AsSpan());
        string[] wildcardNames = wildcard.Select(static p => IOPath.GetFileName(p)!).OrderBy(static n => n).ToArray();
        await Assert.That(wildcardNames).IsEquivalentTo(["a.txt", "alpha.txt", "b.log", "beta.txt", "exact.txt", "exact.txtx", "item.txt", "notes.txt", "one.bin", "report.new", "report.old", "two.bin"]);

        string[] txtOnly = SpanDirectory.GetFiles(temp.Path.AsSpan(), "*.txt".AsSpan());
        string[] txtNames = txtOnly.Select(static p => IOPath.GetFileName(p)!).OrderBy(static n => n).ToArray();
        await Assert.That(txtNames).IsEquivalentTo(["a.txt", "alpha.txt", "beta.txt", "exact.txt", "item.txt", "notes.txt"]);

        string[] prefix = SpanDirectory.GetFiles(temp.Path.AsSpan(), "al*".AsSpan());
        await Assert.That(prefix.Select(static p => IOPath.GetFileName(p)!).ToArray()).IsEquivalentTo(["alpha.txt"]);

        string[] suffix = SpanDirectory.GetFiles(temp.Path.AsSpan(), "*old".AsSpan());
        await Assert.That(suffix.Select(static p => IOPath.GetFileName(p)!).ToArray()).IsEquivalentTo(["report.old"]);

        string[] exact = SpanDirectory.GetFiles(temp.Path.AsSpan(), "exact.txt".AsSpan());
        await Assert.That(exact.Select(static p => IOPath.GetFileName(p)!).ToArray()).IsEquivalentTo(["exact.txt"]);

        string[] defaultPattern = SpanDirectory.GetFiles(temp.Path.AsSpan());
        await Assert.That(defaultPattern.Length).IsGreaterThanOrEqualTo(2);

        using TempDirectoryScope empty = new();
        await Assert.That(SpanDirectory.GetFiles(empty.Path.AsSpan(), "*".AsSpan()).Length).IsEqualTo(0);

        string[] fullPath = SpanDirectory.GetFiles(temp.Path.AsSpan(), "item.txt".AsSpan());
        await Assert.That(fullPath.Length).IsEqualTo(1);
        await Assert.That(fullPath[0]).IsEqualTo(itemPath);

        if (!OperatingSystem.IsWindows()) {
            using TempDirectoryScope hiddenTemp = new();
            hiddenTemp.CreateFile(".hidden", "secret".AsSpan());
            hiddenTemp.CreateFile("visible.txt", "v".AsSpan());
            string[] hidden = SpanDirectory.GetFiles(hiddenTemp.Path.AsSpan(), "*".AsSpan());
            string[] hiddenNames = hidden.Select(static p => IOPath.GetFileName(p)!).OrderBy(static n => n).ToArray();
            await Assert.That(hiddenNames).IsEquivalentTo([".hidden", "visible.txt"]);
        }

        for (int i = 0; i < 40; i++) {
            temp.CreateEmptyFile($"file-{i:D3}.dat");
        }

        await Assert.That(SpanDirectory.GetFiles(temp.Path.AsSpan(), "*.dat".AsSpan()).Length).IsEqualTo(40);

        SpanIOException? emptyPath = null;
        try {
            SpanDirectory.GetFiles(ReadOnlySpan<char>.Empty);
        }
        catch (SpanIOException ex) {
            emptyPath = ex;
        }

        await Assert.That(emptyPath).IsNotNull();

        Exception? missing = null;
        try {
            SpanDirectory.GetFiles(temp.Combine("not-created").AsSpan());
        }
        catch (Exception ex) {
            missing = ex;
        }

        await Assert.That(missing).IsNotNull();
    }

    [Test]
    public async Task GetFiles_ManyFilesAndLongPath() {
        using DirectoryListingFixture fixture = new(PlatformPathBuffer.STACK_THRESHOLD_CHARS + 1);
        for (int i = 0; i < 40; i++) {
            fixture.CreateEmptyFile($"file-{i:D3}.dat");
        }

        string longPath = fixture.CreateFile("f.txt", "x".AsSpan());

        await Assert.That(SpanDirectory.GetFiles(fixture.ListPathSpan, "*.dat".AsSpan()).Length).IsEqualTo(40);

        string[] longMatches = SpanDirectory.GetFiles(fixture.ListPathSpan, "f.txt".AsSpan());
        await Assert.That(longMatches.Length).IsEqualTo(1);
        await Assert.That(longMatches[0]).IsEqualTo(longPath);
        await Assert.That(longPath.Length).IsGreaterThan(PlatformPathBuffer.STACK_THRESHOLD_CHARS);
    }

    [Test]
    public async Task EnumerateFiles() {
        using TempDirectoryScope temp = new();
        temp.CreateFile("a.txt", "a".AsSpan());
        temp.CreateFile("b.txt", "b".AsSpan());
        temp.CreateFile("c.log", "c".AsSpan());

        string[] fromGetFiles = SpanDirectory.GetFiles(temp.Path.AsSpan(), "*.txt".AsSpan());
        List<string> fromEnumerate = [];
        char[] buffer = new char[256];
        SpanDirectoryEntryEnumerator enumerator = SpanDirectory.EnumerateFiles(temp.Path.AsSpan(), buffer, "*.txt".AsSpan());
        try {
            while (enumerator.MoveNext()) {
                fromEnumerate.Add(enumerator.Current.ToString());
            }
        } finally {
            enumerator.Dispose();
        }

        await Assert.That(fromEnumerate.OrderBy(static path => path))
            .IsEquivalentTo(fromGetFiles.OrderBy(static path => path));

        await Assert.That(SpanDirectory.EnumerateFiles(temp.Path.AsSpan()).Count()).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task EnumerateFiles_Foreach() {
        using TempDirectoryScope temp = new();
        temp.CreateFile("a.txt", "a".AsSpan());
        temp.CreateFile("b.txt", "b".AsSpan());
        temp.CreateFile("c.log", "c".AsSpan());

        string[] fromGetFiles = SpanDirectory.GetFiles(temp.Path.AsSpan(), "*.txt".AsSpan());
        List<string> fromForeach = [];
        Span<char> buffer = stackalloc char[256];
        foreach (ReadOnlySpan<char> path in SpanDirectory.EnumerateFiles(temp.Path.AsSpan(), buffer, "*.txt".AsSpan())) {
            fromForeach.Add(path.ToString());
        }

        await Assert.That(fromForeach.OrderBy(static path => path))
            .IsEquivalentTo(fromGetFiles.OrderBy(static path => path));
    }

    [Test]
    [Arguments(2, false)]
    [Arguments(40, false)]
    [Arguments(40, true)]
    public async Task EnumerateFiles_WithEntryBuffer(int fileCount, bool includeLongPath) {
        int minDirectoryPathLength = includeLongPath ? PlatformPathBuffer.STACK_THRESHOLD_CHARS + 1 : 0;
        using DirectoryListingFixture fixture = new(minDirectoryPathLength);
        DirectoryEnumerationTestHelpers.CreateNumberedTxtFiles(fixture, fileCount);

        List<string> fileNames = Enumerable.Range(0, fileCount).Select(static i => $"file{i}.txt").ToList();
        int entryBufferLength = DirectoryEnumerationTestHelpers.GetMaxEntryPathLength(fixture.ListPath, fileNames);
        char[] entryBuffer = new char[entryBufferLength];
        SpanDirectoryEntryEnumerator enumerator =
            SpanDirectory.EnumerateFiles(fixture.ListPathSpan, entryBuffer, "*.txt".AsSpan());
        List<string> names = DirectoryEnumerationTestHelpers.CollectFileNames(enumerator);
        enumerator.Dispose();

        await DirectoryEnumerationTestHelpers.AssertEnumerateFileNamesMatchGetFiles(fixture, names, includeLongPath);
    }

    [Test]
    public async Task EnumerateFiles_WithUndersizedEntryBuffer_StopsAtLongPath() {
        using DirectoryListingFixture fixture = new(PlatformPathBuffer.STACK_THRESHOLD_CHARS + 1);
        fixture.CreateFile("f.txt", "x".AsSpan());

        char[] entryBuffer = new char[PlatformPathBuffer.STACK_THRESHOLD_CHARS];
        SpanDirectoryEntryEnumerator enumerator =
            SpanDirectory.EnumerateFiles(fixture.ListPathSpan, entryBuffer, "*.txt".AsSpan());
        bool moved = enumerator.MoveNext();
        enumerator.Dispose();

        await Assert.That(moved).IsFalse();
    }

    [Test]
    [Arguments(2, false)]
    [Arguments(40, false)]
    [Arguments(40, true)]
    public async Task EnumerateFiles_WithBuffer(int fileCount, bool includeLongPath) {
        int minDirectoryPathLength = includeLongPath ? PlatformPathBuffer.STACK_THRESHOLD_CHARS + 1 : 0;
        using DirectoryListingFixture fixture = new(minDirectoryPathLength);
        DirectoryEnumerationTestHelpers.CreateNumberedTxtFiles(fixture, fileCount);

        char[] buffer = new char[DirectoryEnumerationTestHelpers.GetMaxEntryPathLength(
            fixture.ListPath,
            Enumerable.Range(0, fileCount).Select(static i => $"file{i}.txt"))];
        List<string> fromEnumerate = [];
        SpanDirectoryEntryEnumerator enumerator =
            SpanDirectory.EnumerateFiles(fixture.ListPathSpan, buffer, "*.txt".AsSpan());
        try {
            while (enumerator.MoveNext()) {
                fromEnumerate.Add(enumerator.Current.ToString());
            }
        } finally {
            enumerator.Dispose();
        }

        string[] fromGetFiles = SpanDirectory.GetFiles(fixture.ListPathSpan, buffer, "*.txt".AsSpan());
        await Assert.That(fromEnumerate.OrderBy(static path => path))
            .IsEquivalentTo(fromGetFiles.OrderBy(static path => path));

        if (includeLongPath) {
            int longest = fromEnumerate.Max(static path => path.Length);
            await Assert.That(longest).IsGreaterThan(PlatformPathBuffer.STACK_THRESHOLD_CHARS);
        }
    }

    [Test]
    public async Task Exists_NullCharacter_ReturnsFalse() {
        bool exists = SpanDirectory.Exists("missing\0dir".AsSpan());

        await Assert.That(exists).IsFalse();
        await Assert.That(Directory.Exists("missing\0dir")).IsFalse();
    }

    [Test]
    public async Task CreateDirectory_NullCharacter_ThrowsArgumentException() {
        SpanIOException? exception = null;
        try {
            SpanDirectory.CreateDirectory("bad\0path".AsSpan());
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }

    [Test]
    public async Task Move() {
        using TempDirectoryScope temp = new();
        string source = temp.Combine("source-dir");
        string destination = temp.Combine("moved-dir");
        SpanDirectory.CreateDirectory(source.AsSpan());

        SpanDirectory.Move(source.AsSpan(), destination.AsSpan());

        await Assert.That(SpanDirectory.Exists(source.AsSpan())).IsFalse();
        await Assert.That(SpanDirectory.Exists(destination.AsSpan())).IsTrue();
    }

    [Test]
    public async Task GetParent_And_GetDirectoryRoot() {
        using TempDirectoryScope temp = new();
        string nested = temp.Combine("a", "b");
        SpanDirectory.CreateDirectory(nested.AsSpan());

        ReadOnlySpan<char> parent = SpanDirectory.GetParent(nested.AsSpan());
        await Assert.That(parent.ToString()).IsEqualTo(Directory.GetParent(nested)!.FullName);

        ReadOnlySpan<char> root = SpanDirectory.GetDirectoryRoot(nested.AsSpan());
        await Assert.That(root.ToString()).IsEqualTo(Directory.GetDirectoryRoot(nested));
    }

    [Test]
    public async Task GetLogicalDrives() {
        string[] viaSpan = SpanDirectory.GetLogicalDrives();
        string[] viaBcl = Directory.GetLogicalDrives();

        await Assert.That(viaSpan.Length).IsEqualTo(viaBcl.Length);
        await Assert.That(viaSpan.OrderBy(static d => d)).IsEquivalentTo(viaBcl.OrderBy(static d => d));
    }

    [Test]
    public async Task EnumerateDirectories() {
        using TempDirectoryScope temp = new();
        SpanDirectory.CreateDirectory(temp.Combine("alpha").AsSpan());
        SpanDirectory.CreateDirectory(temp.Combine("beta").AsSpan());
        temp.CreateFile("file.txt", "x".AsSpan());

        string[] directories = SpanDirectory.GetDirectories(temp.Path.AsSpan());
        IEnumerable<string> enumerated = SpanDirectory.EnumerateDirectories(temp.Path.AsSpan());

        await Assert.That(enumerated.Count()).IsEqualTo(directories.Length);
        await Assert.That(enumerated.Select(static p => IOPath.GetFileName(p)).OrderBy(static n => n))
            .IsEquivalentTo(["alpha", "beta"]);
    }

    [Test]
    public async Task GetAndSetLastWriteTimeUtc() {
        using TempDirectoryScope temp = new();
        DateTime expected = new(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        SpanDirectory.SetLastWriteTimeUtc(temp.Path.AsSpan(), expected);
        DateTime actual = SpanDirectory.GetLastWriteTimeUtc(temp.Path.AsSpan());

        await Assert.That(actual).IsEqualTo(expected);
        await Assert.That(Directory.GetLastWriteTimeUtc(temp.Path)).IsEqualTo(expected);
    }

    [Test]
    public async Task SetAttributes() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        using TempDirectoryScope temp = new();

        SpanDirectory.SetAttributes(temp.Path.AsSpan(), FileAttributes.ReadOnly);
        await Assert.That(File.GetAttributes(temp.Path).HasFlag(FileAttributes.ReadOnly)).IsTrue();

        SpanDirectory.SetAttributes(temp.Path.AsSpan(), FileAttributes.Normal);
    }

    [Test]
    public async Task SetLastAccessTimeUtc() {
        using TempDirectoryScope temp = new();
        DateTime expected = new(2023, 1, 15, 8, 30, 0, DateTimeKind.Utc);

        SpanDirectory.SetLastAccessTimeUtc(temp.Path.AsSpan(), expected);

        await Assert.That(SpanDirectory.GetLastAccessTimeUtc(temp.Path.AsSpan())).IsEqualTo(expected);
        await Assert.That(Directory.GetLastAccessTimeUtc(temp.Path)).IsEqualTo(expected);
    }

    [Test]
    public async Task SetUnixFileMode() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        using TempDirectoryScope temp = new();
        UnixFileMode expected = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead;

        SpanDirectory.SetUnixFileMode(temp.Path.AsSpan(), expected);

        await Assert.That(SpanDirectory.GetUnixFileMode(temp.Path.AsSpan())).IsEqualTo(expected);
        await Assert.That(File.GetUnixFileMode(temp.Path)).IsEqualTo(expected);
    }
}
