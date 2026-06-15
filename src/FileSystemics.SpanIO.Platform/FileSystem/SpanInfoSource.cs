namespace FileSystemics.IO;

internal static class SpanInfoSource {
    internal static SpanFileInfo File(ReadOnlySpan<char> path) => new(path);

    internal static SpanDirectoryInfo Directory(ReadOnlySpan<char> path) => new(path);

    internal static SpanDriveInfo Drive(ReadOnlySpan<char> path) => new(path);
}
