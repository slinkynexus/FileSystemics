namespace FileSystemics.SpanIO.Tests;

using IO;
using IOPath = Path;

public class SpanPathTests {

    [Test]
    public async Task DirectorySeparatorChar_MatchesPlatform() {
        char expected = OperatingSystem.IsWindows() ? '\\' : '/';
        await Assert.That(SpanPath.DirectorySeparatorChar).IsEqualTo(expected);
    }

    [Test]
    public async Task AltDirectorySeparatorChar_MatchesPlatform() {
        char expected = OperatingSystem.IsWindows() ? '/' : '/';
        await Assert.That(SpanPath.AltDirectorySeparatorChar).IsEqualTo(expected);
    }

    [Test]
    public async Task VolumeSeparatorChar_IsColon() {
        await Assert.That(SpanPath.VolumeSeparatorChar).IsEqualTo(':');
    }

    [Test]
    public async Task PathSeparator_MatchesPlatform() {
        char expected = OperatingSystem.IsWindows() ? ';' : ':';
        await Assert.That(SpanPath.PathSeparator).IsEqualTo(expected);
    }

    [Test]
    [Arguments("/var/data/hello.txt")]
    [Arguments(@"var\data\hello.txt")]
    [Arguments(@"C:\var\data\hello.txt")]
    [Arguments("hello.txt")]
    [Arguments("")]
    [Arguments("/tmp/")]
    [Arguments(@"tmp\")]
    [Arguments(@"C:\tmp\")]
    [Arguments("no-separators")]
    [Arguments("/var/data/nested/")]
    [Arguments(@"var\data\nested\")]
    [Arguments(@"C:\var\data\nested\")]
    public async Task GetFileName(string path) {
        string actual = SpanPath.GetFileName(path.AsSpan()).ToString();
        await Assert.That(actual).IsEqualTo(IOPath.GetFileName(path));
    }

    [Test]
    [Arguments("/var/data/hello.txt")]
    [Arguments(@"var\data\hello.txt")]
    [Arguments(@"C:\var\data\hello.txt")]
    [Arguments("")]
    [Arguments("file.txt")]
    [Arguments("/var/data/")]
    [Arguments(@"var\data\")]
    [Arguments(@"C:\var\data\")]
    [Arguments("/tmp")]
    [Arguments(@"\tmp")]
    [Arguments(@"C:\tmp")]
    [Arguments("/")]
    public async Task GetDirectoryName(string path) {
        string actual = SpanPath.GetDirectoryName(path.AsSpan()).ToString();
        await Assert.That(actual).IsEqualTo(IOPath.GetDirectoryName(path) ?? string.Empty);
    }

    [Test]
    [Arguments("/tmp/archive.tar.gz")]
    [Arguments(@"tmp\archive.tar.gz")]
    [Arguments(@"C:\tmp\archive.tar.gz")]
    [Arguments("/tmp/README")]
    [Arguments(@"tmp\README")]
    [Arguments(@"C:\tmp\README")]
    [Arguments("/tmp/.gitignore")]
    [Arguments(@"tmp\.gitignore")]
    [Arguments(@"C:\tmp\.gitignore")]
    [Arguments("/tmp/file.")]
    [Arguments(@"tmp\file.")]
    [Arguments(@"C:\tmp\file.")]
    [Arguments("file.txt")]
    [Arguments("no-dot")]
    public async Task GetExtension(string path) {
        string actual = SpanPath.GetExtension(path.AsSpan()).ToString();
        await Assert.That(actual).IsEqualTo(IOPath.GetExtension(path));
    }

    [Test]
    [Arguments("/tmp/report.pdf")]
    [Arguments(@"tmp\report.pdf")]
    [Arguments(@"C:\tmp\report.pdf")]
    [Arguments("/tmp/.gitignore")]
    [Arguments(@"tmp\.gitignore")]
    [Arguments(@"C:\tmp\.gitignore")]
    [Arguments("/tmp/archive.tar.gz")]
    [Arguments(@"tmp\archive.tar.gz")]
    [Arguments(@"C:\tmp\archive.tar.gz")]
    [Arguments("/tmp/file.")]
    [Arguments(@"tmp\file.")]
    [Arguments(@"C:\tmp\file.")]
    [Arguments("name")]
    [Arguments("/tmp/noext")]
    [Arguments(@"tmp\noext")]
    [Arguments(@"C:\tmp\noext")]
    public async Task GetFileNameWithoutExtension(string path) {
        string actual = SpanPath.GetFileNameWithoutExtension(path.AsSpan()).ToString();
        await Assert.That(actual).IsEqualTo(IOPath.GetFileNameWithoutExtension(path));
    }

    [Test]
    [Arguments("/var/data/file.txt")]
    [Arguments(@"\var\data\file.txt")]
    [Arguments(@"C:\var\data\file.txt")]
    [Arguments("relative/file.txt")]
    [Arguments(@"relative\file.txt")]
    [Arguments("")]
    public async Task GetPathRoot(string path) {
        string actual = SpanPath.GetPathRoot(path.AsSpan()).ToString();
        await Assert.That(actual).IsEqualTo(IOPath.GetPathRoot(path) ?? string.Empty);
    }

    [Test]
    [Arguments("/tmp/file.txt")]
    [Arguments(@"tmp\file.txt")]
    [Arguments(@"C:\tmp\file.txt")]
    [Arguments("/tmp/file")]
    [Arguments(@"tmp\file")]
    [Arguments(@"C:\tmp\file")]
    [Arguments("/tmp/.hidden")]
    [Arguments(@"tmp\.hidden")]
    [Arguments(@"C:\tmp\.hidden")]
    [Arguments("/tmp/file.")]
    [Arguments(@"tmp\file.")]
    [Arguments(@"C:\tmp\file.")]
    [Arguments("")]
    public async Task HasExtension(string path) {
        await Assert.That(SpanPath.HasExtension(path.AsSpan())).IsEqualTo(IOPath.HasExtension(path));
    }

    [Test]
    [Arguments("/absolute/path")]
    [Arguments(@"absolute\path")]
    [Arguments(@"C:\absolute\path")]
    [Arguments("C:relative")]
    [Arguments("relative/path")]
    [Arguments(@"relative\path")]
    [Arguments("")]
    public async Task IsPathRooted(string path) {
        await Assert.That(SpanPath.IsPathRooted(path.AsSpan())).IsEqualTo(IOPath.IsPathRooted(path));
    }

    [Test]
    [Arguments("/absolute/path")]
    [Arguments(@"absolute\path")]
    [Arguments(@"C:\absolute\path")]
    [Arguments("C:relative")]
    [Arguments("relative")]
    [Arguments("")]
    public async Task IsPathFullyQualified(string path) {
        await Assert.That(SpanPath.IsPathFullyQualified(path.AsSpan())).IsEqualTo(IOPath.IsPathFullyQualified(path));
    }

    [Test]
    [Arguments("folder/")]
    [Arguments(@"folder\")]
    [Arguments("folder")]
    [Arguments("")]
    [Arguments("/tmp/")]
    [Arguments(@"tmp\")]
    [Arguments(@"C:\tmp\")]
    public async Task EndsInDirectorySeparator(string path) {
        await Assert.That(SpanPath.EndsInDirectorySeparator(path.AsSpan())).IsEqualTo(IOPath.EndsInDirectorySeparator(path));
    }

    [Test]
    [Arguments("folder/")]
    [Arguments(@"folder\")]
    [Arguments("folder")]
    [Arguments("/tmp/")]
    [Arguments(@"tmp\")]
    [Arguments(@"C:\tmp\")]
    [Arguments("/")]
    public async Task TrimEndingDirectorySeparator(string path) {
        string actual = SpanPath.TrimEndingDirectorySeparator(path.AsSpan()).ToString();
        await Assert.That(actual).IsEqualTo(IOPath.TrimEndingDirectorySeparator(path));
    }

    [Test]
    [Arguments("/tmp", "file.txt")]
    [Arguments("tmp", "file.txt")]
    [Arguments(@"C:\tmp", "file.txt")]
    [Arguments("/tmp/", "file.txt")]
    [Arguments(@"tmp\", "file.txt")]
    [Arguments(@"C:\tmp\", "file.txt")]
    [Arguments("", "file.txt")]
    [Arguments("", "")]
    [Arguments("a", "b")]
    [Arguments("a", "b")]
    public async Task TryJoin(string path1, string path2) {
        string expected = IOPath.Join(path1, path2);
        int length = expected.Length;
        Span<char> destination = stackalloc char[length];
        bool success = SpanPath.TryJoin(path1.AsSpan(), path2.AsSpan(), destination, out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(charsWritten).IsEqualTo(length);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TryJoin_DestinationTooSmall_ReturnsFalse() {
        Span<char> destination = stackalloc char[4];
        bool success = SpanPath.TryJoin("/tmp".AsSpan(), "file.txt".AsSpan(), destination, out int charsWritten);
        await Assert.That(success).IsFalse();
        await Assert.That(charsWritten).IsEqualTo(0);
    }

    [Test]
    [Arguments("/tmp", "nested", "file.txt")]
    [Arguments("tmp", "nested", "file.txt")]
    [Arguments(@"C:\tmp", "nested", "file.txt")]
    [Arguments("", "nested", "file.txt")]
    [Arguments("", "nested", "file.txt")]
    [Arguments("/tmp", "", "file.txt")]
    [Arguments("tmp", "", "file.txt")]
    [Arguments(@"C:\tmp", "", "file.txt")]
    [Arguments("", "", "")]
    public async Task TryJoin_ThreePaths(string path1, string path2, string path3) {
        string expected = IOPath.Join(path1, path2, path3);
        int length = expected.Length;
        Span<char> destination = stackalloc char[length];
        bool success = SpanPath.TryJoin(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), destination, out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(charsWritten).IsEqualTo(length);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TryJoin_ThreePaths_DestinationTooSmall_ReturnsFalse() {
        Span<char> destination = stackalloc char[8];
        bool success = SpanPath.TryJoin(
            "/tmp".AsSpan(),
            "nested".AsSpan(),
            "file.txt".AsSpan(),
            destination,
            out int charsWritten);
        await Assert.That(success).IsFalse();
        await Assert.That(charsWritten).IsEqualTo(0);
    }

    [Test]
    [Arguments("base", "child")]
    [Arguments("base", "child")]
    [Arguments("/var/data", "/etc/hosts")]
    [Arguments(@"\var\data", @"\etc\hosts")]
    [Arguments(@"C:\var\data", @"C:\etc\hosts")]
    [Arguments(@"C:\var\data", @"D:\etc\hosts")]
    [Arguments("", "child")]
    public async Task TryCombine(string path1, string path2) {
        string expected = IOPath.Combine(path1, path2);
        int capacity = Math.Max(expected.Length, path2.Length);
        Span<char> destination = stackalloc char[capacity];
        bool success = SpanPath.TryCombine(path1.AsSpan(), path2.AsSpan(), destination, out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TryCombine_RootedSecondSegment_DestinationTooSmall_ReturnsFalse() {
        Span<char> destination = stackalloc char[4];
        bool success = SpanPath.TryCombine("base".AsSpan(), "/etc/hosts".AsSpan(), destination, out int charsWritten);
        await Assert.That(success).IsFalse();
        await Assert.That(charsWritten).IsEqualTo(0);
    }

    [Test]
    [Arguments("a", "b", "c")]
    [Arguments("a", "b", "c")]
    public async Task TryCombine_ThreePaths(string path1, string path2, string path3) {
        string expected = IOPath.Combine(path1, path2, path3);
        int capacity = expected.Length;
        Span<char> destination = stackalloc char[capacity];
        bool success = SpanPath.TryCombine(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), destination, out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TryCombine_ThreePaths_RootedMiddleSegment_ReplacesPrior() {
        string rooted = TestPaths.Rooted("abs");
        string expected = IOPath.Combine("a", rooted, "c");
        int capacity = expected.Length;
        Span<char> destination = stackalloc char[capacity];
        bool success = SpanPath.TryCombine(
            "a".AsSpan(),
            rooted.AsSpan(),
            "c".AsSpan(),
            destination,
            out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TryCombine_ThreePaths_RootedLastSegment_Wins() {
        string rooted = TestPaths.Rooted("abs");
        int capacity = rooted.Length;
        Span<char> destination = stackalloc char[capacity];
        bool success = SpanPath.TryCombine(
            "a".AsSpan(),
            "b".AsSpan(),
            rooted.AsSpan(),
            destination,
            out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(actual).IsEqualTo(rooted);
    }

    [Test]
    [Arguments("/tmp/file.txt", ".md")]
    [Arguments(@"tmp\file.txt", ".md")]
    [Arguments(@"C:\tmp\file.txt", ".md")]
    [Arguments("/tmp/file.txt", "md")]
    [Arguments(@"tmp\file.txt", "md")]
    [Arguments(@"C:\tmp\file.txt", "md")]
    [Arguments("/tmp/file.txt", "")]
    [Arguments(@"tmp\file.txt", "")]
    [Arguments(@"C:\tmp\file.txt", "")]
    [Arguments("/tmp/.hidden", ".txt")]
    [Arguments(@"tmp\.hidden", ".txt")]
    [Arguments(@"C:\tmp\.hidden", ".txt")]
    [Arguments("/tmp/noext", ".json")]
    [Arguments(@"tmp\noext", ".json")]
    [Arguments(@"C:\tmp\noext", ".json")]
    [Arguments("file", "md")]
    [Arguments("file.", "md")]
    public async Task TryChangeExtension(string path, string extension) {
        string expected = IOPath.ChangeExtension(path, extension) ?? string.Empty;
        int capacity = path.Length + extension.Length + 2;
        Span<char> buffer = stackalloc char[capacity];
        SpanPath.TryChangeExtension(path.AsSpan(), extension.AsSpan(), buffer, out int written);
        string actual = buffer[..written].ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [Arguments("/tmp/file.md", "txt")]
    [Arguments(@"tmp\file.md", "txt")]
    [Arguments(@"C:\tmp\file.md", "txt")]
    [Arguments("/tmp/file.md", ".txt")]
    [Arguments(@"tmp\file.md", ".txt")]
    [Arguments(@"C:\tmp\file.md", ".txt")]
    public async Task TryChangeExtension_DestinationTooSmall_ReturnsFalse(string path, string extension) {
        Span<char> destination = stackalloc char[4];
        bool success = SpanPath.TryChangeExtension(
            path.AsSpan(),
            extension.AsSpan(),
            destination,
            out int charsWritten);
        await Assert.That(success).IsFalse();
        await Assert.That(charsWritten).IsEqualTo(0);
    }

    [Test]
    [Arguments("/var/data", "/var/data")]
    [Arguments(@"\var\data", @"\var\data")]
    [Arguments(@"C:\var\data", @"C:\var\data")]
    [Arguments("/var/data", "/var/data/file.txt")]
    [Arguments(@"\var\data", @"\var\data\file.txt")]
    [Arguments(@"C:\var\data", @"C:\var\data\file.txt")]
    [Arguments("/var/a/b", "/var/a/c")]
    [Arguments(@"\var\a\b", @"\var\a\c")]
    [Arguments(@"C:\var\a\b", @"C:\var\a\c")]
    [Arguments("/var/a", "/var/b")]
    [Arguments(@"\var\a", @"\var\b")]
    [Arguments(@"C:\var\a", @"C:\var\b")]
    [Arguments(@"C:\var\a", @"D:\var\b")]
    public async Task TryGetRelativePath(string relativeTo, string path) {
        string expected = IOPath.GetRelativePath(relativeTo, path);
        int capacity = Math.Max(relativeTo.Length + path.Length + 8, expected.Length);
        Span<char> destination = stackalloc char[capacity];
        bool success = SpanPath.TryGetRelativePath(relativeTo.AsSpan(), path.AsSpan(), destination, out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task TryGetRelativePath_EmptyRelativeTo_ReturnsFalse() {
        Span<char> destination = stackalloc char[16];
        bool success = SpanPath.TryGetRelativePath(
            ReadOnlySpan<char>.Empty,
            "/tmp/a".AsSpan(),
            destination,
            out int charsWritten);
        await Assert.That(success).IsFalse();
        await Assert.That(charsWritten).IsEqualTo(0);
    }

    [Test]
    public async Task TryGetRelativePath_EmptyPath_ReturnsFalse() {
        Span<char> destination = stackalloc char[16];
        bool success = SpanPath.TryGetRelativePath(
            "/tmp".AsSpan(),
            ReadOnlySpan<char>.Empty,
            destination,
            out int charsWritten);
        await Assert.That(success).IsFalse();
        await Assert.That(charsWritten).IsEqualTo(0);
    }

    [Test]
    public async Task TryGetRelativePath_DestinationTooSmall_ReturnsFalse() {
        Span<char> destination = stackalloc char[1];
        bool success = SpanPath.TryGetRelativePath(
            "/var/data".AsSpan(),
            "/var/data/child".AsSpan(),
            destination,
            out int charsWritten);
        await Assert.That(success).IsFalse();
        await Assert.That(charsWritten).IsEqualTo(0);
    }

    [Test]
    public async Task TryGetRelativePath_DifferentRoots_ReturnsFullPath() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        string path = @"D:\other\file.txt";
        Span<char> destination = stackalloc char[path.Length];
        bool success = SpanPath.TryGetRelativePath(@"C:\base".AsSpan(), path.AsSpan(), destination, out int charsWritten);
        string actual = destination[..charsWritten].ToString();
        await Assert.That(success).IsTrue();
        await Assert.That(actual).IsEqualTo(path);
    }

    [Test]
    public async Task TryGetRelativePath_MatchesBclForResolvedPaths() {
        using TempDirectoryScope temp = new();
        string from = temp.Combine("from");
        string to = temp.Combine("from", "child", "target.txt");
        Directory.CreateDirectory(IOPath.GetDirectoryName(to)!);

        string expected = IOPath.GetRelativePath(from, to);
        Span<char> buffer = stackalloc char[from.Length + to.Length + 8];
        string actual = SpanPath.GetRelativePath(from.AsSpan(), to.AsSpan(), buffer).ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [Arguments("/tmp", "file.txt")]
    [Arguments("tmp", "file.txt")]
    [Arguments(@"C:\tmp", "file.txt")]
    [Arguments("/tmp/", "file.txt")]
    [Arguments(@"tmp\", "file.txt")]
    [Arguments(@"C:\tmp\", "file.txt")]
    [Arguments("", "file.txt")]
    [Arguments("", "")]
    [Arguments("a", "b")]
    [Arguments("a", "b")]
    public async Task Join(string path1, string path2) {
        string expected = IOPath.Join(path1, path2);
        int length = expected.Length;
        Span<char> destination = stackalloc char[length];
        string actual = SpanPath.Join(path1.AsSpan(), path2.AsSpan(), destination).ToString();
        await Assert.That(actual.Length).IsEqualTo(length);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task Join_DestinationTooSmall_Throws() {
        Span<char> destination = stackalloc char[4];
        SpanIOException? exception = null;
        try {
            SpanPath.Join("/tmp".AsSpan(), "file.txt".AsSpan(), destination);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("destination");
    }

    [Test]
    [Arguments("/tmp", "nested", "file.txt")]
    [Arguments("tmp", "nested", "file.txt")]
    [Arguments(@"C:\tmp", "nested", "file.txt")]
    [Arguments("", "nested", "file.txt")]
    [Arguments("", "nested", "file.txt")]
    [Arguments("/tmp", "", "file.txt")]
    [Arguments("tmp", "", "file.txt")]
    [Arguments(@"C:\tmp", "", "file.txt")]
    [Arguments("", "", "")]
    public async Task Join_ThreePaths(string path1, string path2, string path3) {
        string expected = IOPath.Join(path1, path2, path3);
        int length = expected.Length;
        Span<char> destination = stackalloc char[length];
        string actual = SpanPath.Join(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), destination).ToString();
        await Assert.That(actual.Length).IsEqualTo(length);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task Join_ThreePaths_DestinationTooSmall_Throws() {
        Span<char> destination = stackalloc char[8];
        SpanIOException? exception = null;
        try {
            SpanPath.Join("/tmp".AsSpan(), "nested".AsSpan(), "file.txt".AsSpan(), destination);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("destination");
    }

    [Test]
    [Arguments("base", "child")]
    [Arguments("base", "child")]
    [Arguments("/var/data", "/etc/hosts")]
    [Arguments(@"\var\data", @"\etc\hosts")]
    [Arguments(@"C:\var\data", @"C:\etc\hosts")]
    [Arguments(@"C:\var\data", @"D:\etc\hosts")]
    [Arguments("", "child")]
    public async Task Combine(string path1, string path2) {
        string expected = IOPath.Combine(path1, path2);
        int capacity = Math.Max(expected.Length, path2.Length);
        Span<char> destination = stackalloc char[capacity];
        string actual = SpanPath.Combine(path1.AsSpan(), path2.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task Combine_DestinationTooSmall_Throws() {
        Span<char> destination = stackalloc char[4];
        SpanIOException? exception = null;
        try {
            SpanPath.Combine("base".AsSpan(), "/etc/hosts".AsSpan(), destination);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("destination");
    }

    [Test]
    [Arguments("a", "b", "c")]
    [Arguments("a", "b", "c")]
    public async Task Combine_ThreePaths(string path1, string path2, string path3) {
        string expected = IOPath.Combine(path1, path2, path3);
        int capacity = Math.Max(expected.Length, path3.Length);
        Span<char> destination = stackalloc char[capacity];
        string actual = SpanPath.Combine(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task Combine_ThreePaths_RootedMiddleSegment_ReplacesPrior() {
        string rooted = TestPaths.Rooted("abs");
        string expected = IOPath.Combine("a", rooted, "c");
        Span<char> destination = stackalloc char[expected.Length];
        string actual = SpanPath.Combine("a".AsSpan(), rooted.AsSpan(), "c".AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task Combine_ThreePaths_RootedLastSegment_Wins() {
        string rooted = TestPaths.Rooted("abs");
        Span<char> destination = stackalloc char[rooted.Length];
        string actual = SpanPath.Combine("a".AsSpan(), "b".AsSpan(), rooted.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(rooted);
    }

    [Test]
    [Arguments("/tmp/file.txt", ".md")]
    [Arguments(@"tmp\file.txt", ".md")]
    [Arguments(@"C:\tmp\file.txt", ".md")]
    [Arguments("/tmp/file.txt", "md")]
    [Arguments(@"tmp\file.txt", "md")]
    [Arguments(@"C:\tmp\file.txt", "md")]
    [Arguments("/tmp/file.txt", "")]
    [Arguments(@"tmp\file.txt", "")]
    [Arguments(@"C:\tmp\file.txt", "")]
    [Arguments("/tmp/.hidden", ".txt")]
    [Arguments(@"tmp\.hidden", ".txt")]
    [Arguments(@"C:\tmp\.hidden", ".txt")]
    [Arguments("/tmp/noext", ".json")]
    [Arguments(@"tmp\noext", ".json")]
    [Arguments(@"C:\tmp\noext", ".json")]
    public async Task ChangeExtension(string path, string extension) {
        string expected = IOPath.ChangeExtension(path, extension) ?? string.Empty;
        int capacity = path.Length + extension.Length + 2;
        Span<char> destination = stackalloc char[capacity];
        string actual = SpanPath.ChangeExtension(path.AsSpan(), extension.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task ChangeExtension_DestinationTooSmall_Throws() {
        Span<char> destination = stackalloc char[4];
        SpanIOException? exception = null;
        try {
            SpanPath.ChangeExtension("/tmp/file.txt".AsSpan(), ".markdown".AsSpan(), destination);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("destination");
    }

    [Test]
    [Arguments("", ".txt")]
    [Arguments("", "")]
    [Arguments("bad\0path.txt", ".md")]
    [Arguments("file.txt", "a\0b")]
    [Arguments("file<.txt", ".md")]
    [Arguments("file|.txt", ".md")]
    [Arguments("file\".txt", ".md")]
    public async Task ChangeExtension_BadPath_Throws(string path, string extension) {
        ArgumentException? bclException = null;
        string? bclResult = null;
        try {
            bclResult = IOPath.ChangeExtension(path, extension);
        }
        catch (ArgumentException ex) {
            bclException = ex;
        }

        int capacity = Math.Max(path.Length + extension.Length + 4, 16);
        Span<char> destination = stackalloc char[capacity];

        if (bclException is not null) {
            SpanIOException? spanException = null;
            try {
                SpanPath.ChangeExtension(path.AsSpan(), extension.AsSpan(), destination);
            }
            catch (SpanIOException ex) {
                spanException = ex;
            }

            await Assert.That(spanException).IsNotNull();
            await Assert.That(spanException!.ParamName).IsEqualTo(bclException.ParamName);
            return;
        }

        string actual = SpanPath.ChangeExtension(path.AsSpan(), extension.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(bclResult ?? string.Empty);
    }

    [Test]
    [Arguments("/var/data", "/var/data")]
    [Arguments(@"\var\data", @"\var\data")]
    [Arguments(@"C:\var\data", @"C:\var\data")]
    [Arguments("/var/data", "/var/data/file.txt")]
    [Arguments(@"\var\data", @"\var\data\file.txt")]
    [Arguments(@"C:\var\data", @"C:\var\data\file.txt")]
    [Arguments("/var/a/b", "/var/a/c")]
    [Arguments(@"\var\a\b", @"\var\a\c")]
    [Arguments(@"C:\var\a\b", @"C:\var\a\c")]
    [Arguments("/var/a", "/var/b")]
    [Arguments(@"\var\a", @"\var\b")]
    [Arguments(@"C:\var\a", @"C:\var\b")]
    [Arguments(@"C:\var\a", @"D:\var\b")]
    public async Task GetRelativePath(string relativeTo, string path) {
        string expected = IOPath.GetRelativePath(relativeTo, path);
        int capacity = Math.Max(relativeTo.Length + path.Length + 8, expected.Length);
        Span<char> destination = stackalloc char[capacity];
        string actual = SpanPath.GetRelativePath(relativeTo.AsSpan(), path.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task GetRelativePath_EmptyRelativeTo_Throws() {
        SpanIOException? exception = null;
        try {
            Span<char> destination = stackalloc char[16];
            SpanPath.GetRelativePath(ReadOnlySpan<char>.Empty, "/tmp/a".AsSpan(), destination);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }
        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("relativeTo");
    }

    [Test]
    public async Task GetRelativePath_EmptyPath_Throws() {
        SpanIOException? exception = null;
        try {
            Span<char> destination = stackalloc char[16];
            SpanPath.GetRelativePath("/tmp".AsSpan(), ReadOnlySpan<char>.Empty, destination);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }

    [Test]
    public async Task GetRelativePath_DestinationTooSmall_Throws() {
        Span<char> destination = stackalloc char[1];
        SpanIOException? exception = null;
        try {
            SpanPath.GetRelativePath("/var/data".AsSpan(), "/var/data/child".AsSpan(), destination);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("destination");
    }

    [Test]
    public async Task GetRelativePath_DifferentRoots_ReturnsFullPath() {
        if (!OperatingSystem.IsWindows()) {
            return;
        }

        string path = @"D:\other\file.txt";
        Span<char> destination = stackalloc char[path.Length];
        string actual = SpanPath.GetRelativePath(@"C:\base".AsSpan(), path.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(path);
    }

    [Test]
    public async Task GetRelativePath_MatchesBclForResolvedPaths() {
        using TempDirectoryScope temp = new();
        string from = temp.Combine("from");
        string to = temp.Combine("from", "child", "target.txt");
        Directory.CreateDirectory(IOPath.GetDirectoryName(to)!);

        string expected = IOPath.GetRelativePath(from, to);
        int capacity = from.Length + to.Length + 8;
        Span<char> destination = stackalloc char[capacity];
        string actual = SpanPath.GetRelativePath(from.AsSpan(), to.AsSpan(), destination).ToString();
        await Assert.That(actual).IsEqualTo(expected);
    }
}
