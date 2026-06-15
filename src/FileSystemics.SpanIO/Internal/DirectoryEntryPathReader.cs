namespace FileSystemics.IO.Internal;

internal static class DirectoryEntryPathReader {
    internal const int MaxEntryNameChars = 260;

    internal static int GetEntryPathBufferCapacity(ReadOnlySpan<char> directoryPath) =>
        directoryPath.Length + MaxEntryNameChars + 1;
}
