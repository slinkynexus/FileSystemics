using FileSystemics.IO;
using FileSystemics.IO.Internal;
using IOPath = System.IO.Path;

namespace FileSystemics.SpanIO.Tests;

internal sealed class DirectoryListingFixture : IDisposable {
    private readonly TempDirectoryScope _temp;

    internal DirectoryListingFixture(int minDirectoryPathLength) {
        _temp = new TempDirectoryScope();
        ListPath = BuildListingDirectory(_temp.Path, minDirectoryPathLength);
    }

    internal string ListPath { get; }

    internal ReadOnlySpan<char> ListPathSpan => ListPath.AsSpan();

    internal string CreateFile(string fileName, ReadOnlySpan<char> content) {
        string relative = Path.GetRelativePath(_temp.Path, Path.Combine(ListPath, fileName));
        return _temp.CreateFile(relative, content);
    }

    internal void CreateEmptyFile(string fileName) {
        string relative = Path.GetRelativePath(_temp.Path, Path.Combine(ListPath, fileName));
        _temp.CreateEmptyFile(relative);
    }

    public void Dispose() => _temp.Dispose();

    private static string BuildListingDirectory(string root, int minPathLength) {
        string current = root;
        int segmentIndex = 0;
        while (current.Length < minPathLength) {
            current = Path.Combine(current, $"d{segmentIndex++}");
            Directory.CreateDirectory(current);
        }

        return current;
    }
}

internal static class DirectoryEnumerationTestHelpers {
    internal static List<string> CollectFileNames(SpanDirectoryEntryEnumerator enumerator) {
        List<string> names = [];
        while (enumerator.MoveNext()) {
            names.Add(SpanPath.GetFileName(enumerator.Current).ToString());
        }

        return names;
    }

    internal static void CreateNumberedTxtFiles(DirectoryListingFixture fixture, int count) {
        for (int i = 0; i < count; i++) {
            fixture.CreateFile($"file{i}.txt", "x".AsSpan());
        }
    }

    internal static int GetMaxEntryPathLength(string directoryPath, IEnumerable<string> fileNames) {
        int max = 0;
        foreach (string fileName in fileNames) {
            max = Math.Max(max, Path.Combine(directoryPath, fileName).Length);
        }

        return max;
    }

    internal static async Task AssertEnumerateFileNamesMatchGetFiles(
        DirectoryListingFixture fixture,
        IReadOnlyList<string> collectedNames,
        bool includeLongPath) {
        string[] expected = SpanDirectory.GetFiles(fixture.ListPathSpan, "*.txt".AsSpan());
        string[] expectedNames = expected.Select(static path => IOPath.GetFileName(path)!).OrderBy(static n => n).ToArray();
        await Assert.That(collectedNames.OrderBy(static name => name)).IsEquivalentTo(expectedNames);

        if (includeLongPath) {
            await Assert.That(fixture.ListPath.Length).IsGreaterThan(PlatformPathBuffer.STACK_THRESHOLD_CHARS);
        }
    }
}

internal sealed class TempDirectoryScope : IDisposable {
    internal TempDirectoryScope() {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"filesystemics-spanio-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    internal string Path { get; }

    internal string Combine(params string[] segments) => System.IO.Path.Combine([Path, .. segments]);

    internal string CreateFile(string relativePath, ReadOnlySpan<char> content) {
        string fullPath = Combine(relativePath);
        string? parent = System.IO.Path.GetDirectoryName(fullPath);
        if (parent is not null && parent.Length > 0) {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(fullPath, content.ToString());
        return fullPath;
    }

    internal string CreateEmptyFile(string relativePath) {
        string fullPath = Combine(relativePath);
        string? parent = System.IO.Path.GetDirectoryName(fullPath);
        if (parent is not null && parent.Length > 0) {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllBytes(fullPath, []);
        return fullPath;
    }

    public void Dispose() {
        if (Directory.Exists(Path)) {
            Directory.Delete(Path, recursive: true);
        }
    }
}

internal static class TestPaths {
    internal static string Rooted(string relative) {
        if (OperatingSystem.IsWindows()) {
            return Path.Combine(@"C:\", relative.TrimStart('\\', '/'));
        }

        return $"/{relative.TrimStart('/')}";
    }
}
