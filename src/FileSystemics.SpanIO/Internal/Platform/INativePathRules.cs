namespace FileSystemics.IO.Internal;

internal interface INativePathRules<T> where T : INativePathRules<T> {
    static abstract char DirectorySeparatorChar { get; }

    static abstract char AltDirectorySeparatorChar { get; }

    static abstract char VolumeSeparatorChar { get; }

    static abstract char PathSeparator { get; }

    static abstract StringComparison PathComparison { get; }

    static abstract bool UsesUtf16NativePaths { get; }

    static abstract bool UsesGetdents64DirectoryEnumeration { get; }

    static abstract int GetRootLength(ReadOnlySpan<char> path);

    static abstract bool IsPathRooted(ReadOnlySpan<char> path);

    static abstract bool IsPartiallyQualified(ReadOnlySpan<char> path);

    static abstract bool IsDirectorySeparator(char value);

    static abstract bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path);

    static abstract bool IsEffectivelyEmpty(ReadOnlySpan<char> path);

    static abstract TResult WithNativePath<TResult>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, TResult> action);

    static abstract TResult WithCombinedNativePath<TResult>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, TResult> action);

    static abstract TResult WithTwoNativePaths<TResult>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, TResult> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, TResult> utf8Action);
}
