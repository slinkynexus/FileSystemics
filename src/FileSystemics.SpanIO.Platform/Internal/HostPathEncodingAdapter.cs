using FileSystemics.IO.Internal;

namespace FileSystemics.IO.PlatformHosts.Internal;

internal sealed class HostPathEncodingAdapter(IPathRules pathRules) : INativePathEncoding {
    public T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
        pathRules.WithNativePath(path, action);

    public T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
        pathRules.WithCombinedNativePath(directory, fileName, separator, action);

    public T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) =>
        pathRules.WithTwoNativePaths(path1, path2, utf16Action, utf8Action);
}
