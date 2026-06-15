using FileSystemics.IO;

// ReSharper disable once CheckNamespace
namespace FileSystemics.SpanIO.Platform.Tests;

public partial class PlatformUseTests {
    private sealed class ForwardSlashPathHost(IPlatformHost inner) : PlatformHost {
        public override IFileSystemPlatform FileSystem => inner.FileSystem;

        public override IPathRules Path { get; } = new SeparatorPathRules(inner.Path, '/', '\\');

        public override IDrivePlatform Drives => inner.Drives;
    }

      private sealed class BackslashPathHost(IPlatformHost inner) : PlatformHost {
        public override IFileSystemPlatform FileSystem => inner.FileSystem;

        public override IPathRules Path { get; } = new SeparatorPathRules(inner.Path, '\\', '/');

        public override IDrivePlatform Drives => inner.Drives;
    }

    private sealed class SeparatorPathRules(IPathRules inner, char separator, char altSeparator) : IPathRules {
        public char DirectorySeparatorChar => separator;

        public char AltDirectorySeparatorChar => altSeparator;

        public char VolumeSeparatorChar => inner.VolumeSeparatorChar;

        public char PathSeparator => inner.PathSeparator;

        public StringComparison PathComparison => inner.PathComparison;

        public bool UsesUtf16NativePaths => inner.UsesUtf16NativePaths;

        public bool UsesGetdents64DirectoryEnumeration => inner.UsesGetdents64DirectoryEnumeration;

        public int GetRootLength(ReadOnlySpan<char> path) {
            if (path.IsEmpty) {
                return 0;
            }

            if (path[0] == separator) {
                return separator == altSeparator ? 1 : GetDrivePrefixedRootLength(path);
            }

            if (path.Length >= 2 && path[1] == inner.VolumeSeparatorChar && char.IsAsciiLetter(path[0])) {
                return path.Length >= 3 && IsDirectorySeparator(path[2]) ? 3 : 2;
            }

            return 0;
        }

        public bool IsPathRooted(ReadOnlySpan<char> path) => GetRootLength(path) != 0;

        public bool IsPartiallyQualified(ReadOnlySpan<char> path) => GetRootLength(path) == 0;

        public bool IsDirectorySeparator(char value) =>
            value == separator || value == altSeparator;

        private int GetDrivePrefixedRootLength(ReadOnlySpan<char> path) {
            if (path.Length >= 2 && path[1] == inner.VolumeSeparatorChar && char.IsAsciiLetter(path[0])) {
                return path.Length >= 3 && IsDirectorySeparator(path[2]) ? 3 : 2;
            }

            return path[0] == separator ? 1 : 0;
        }

        public bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path) =>
            inner.ShouldExistenceCheckReturnFalse(path);

        public bool IsEffectivelyEmpty(ReadOnlySpan<char> path) => inner.IsEffectivelyEmpty(path);

        public T WithNativePath<T>(
            ReadOnlySpan<char> path,
            Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
            inner.WithNativePath(path, action);

        public T WithCombinedNativePath<T>(
            ReadOnlySpan<char> directory,
            ReadOnlySpan<char> fileName,
            char pathSeparator,
            Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
            inner.WithCombinedNativePath(directory, fileName, pathSeparator, action);

        public T WithTwoNativePaths<T>(
            ReadOnlySpan<char> path1,
            ReadOnlySpan<char> path2,
            Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
            Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) =>
            inner.WithTwoNativePaths(path1, path2, utf16Action, utf8Action);
    }
}
