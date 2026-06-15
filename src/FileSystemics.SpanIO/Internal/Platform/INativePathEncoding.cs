namespace FileSystemics.IO.Internal;

internal interface INativePathEncoding {
    T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action);

    T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action);

    T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action);
}
