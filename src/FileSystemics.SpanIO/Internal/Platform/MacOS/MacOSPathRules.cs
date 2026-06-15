namespace FileSystemics.IO.Internal;

internal sealed class MacOSPathRules : INativePathRules<MacOSPathRules> {
    private MacOSPathRules() {
    }

    public static char DirectorySeparatorChar => '/';

    public static char AltDirectorySeparatorChar => '/';

    public static char VolumeSeparatorChar => ':';

    public static char PathSeparator => ':';

    public static StringComparison PathComparison => StringComparison.OrdinalIgnoreCase;

    public static bool UsesUtf16NativePaths => false;

    public static bool UsesGetdents64DirectoryEnumeration => false;

    public static int GetRootLength(ReadOnlySpan<char> path) =>
        !path.IsEmpty && path[0] == DirectorySeparatorChar ? 1 : 0;

    public static bool IsPathRooted(ReadOnlySpan<char> path) =>
        !path.IsEmpty && path[0] == DirectorySeparatorChar;

    public static bool IsPartiallyQualified(ReadOnlySpan<char> path) =>
        path.IsEmpty || path[0] != DirectorySeparatorChar;

    public static bool IsDirectorySeparator(char value) =>
        value == DirectorySeparatorChar || value == AltDirectorySeparatorChar;

    public static bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path) =>
        path.IsEmpty || path.Contains('\0');

    public static bool IsEffectivelyEmpty(ReadOnlySpan<char> path) => path.IsEmpty;

    public static T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
        PosixPathEncoding.WithNativePath(path, action);

    public static T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
        PosixPathEncoding.WithCombinedNativePath(directory, fileName, separator, action);

    public static T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) =>
        PosixPathEncoding.WithTwoNativePaths(path1, path2, utf16Action, utf8Action);
}
