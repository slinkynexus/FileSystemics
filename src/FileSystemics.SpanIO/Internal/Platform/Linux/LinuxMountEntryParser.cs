using System.Text;

namespace FileSystemics.IO.Internal;

internal static class LinuxMountEntryParser {
    internal static bool TryGetMountPoint(ReadOnlySpan<byte> line, Span<char> destination, out int charsWritten) {
        if (!TryGetFields(line, out ReadOnlySpan<byte> mountPoint, out _)) {
            charsWritten = 0;
            return false;
        }

        return TryDecodeUtf8(mountPoint, destination, out charsWritten);
    }

    internal static bool TryGetMountEntry(
        ReadOnlySpan<byte> line,
        Span<char> mountPointDestination,
        out int mountPointLength,
        Span<char> fileSystemDestination,
        out int fileSystemLength) {
        mountPointLength = 0;
        fileSystemLength = 0;
        if (!TryGetFields(line, out ReadOnlySpan<byte> mountPoint, out ReadOnlySpan<byte> fileSystemType)) {
            return false;
        }

        return TryDecodeUtf8(mountPoint, mountPointDestination, out mountPointLength) &&
               TryDecodeUtf8(fileSystemType, fileSystemDestination, out fileSystemLength);
    }

    internal static bool TryGetFields(
        ReadOnlySpan<byte> line,
        out ReadOnlySpan<byte> mountPoint,
        out ReadOnlySpan<byte> fileSystemType) {
        mountPoint = default;
        fileSystemType = default;
        if (line.IsEmpty || line[0] == (byte)'#') {
            return false;
        }

        int fieldIndex = 0;
        int fieldStart = -1;
        bool escaped = false;

        for (int i = 0; i <= line.Length; i++) {
            bool atEnd = i == line.Length;
            byte ch = atEnd ? (byte)' ' : line[i];

            if (!atEnd && !escaped && ch == (byte)'\\') {
                escaped = true;
                continue;
            }

            if (atEnd || (!escaped && ch == (byte)' ')) {
                if (fieldStart >= 0 && i > fieldStart) {
                    ReadOnlySpan<byte> field = line[fieldStart..i];
                    switch (fieldIndex) {
                        case 1:
                            mountPoint = field;
                            break;
                        case 2:
                            fileSystemType = field;
                            break;
                    }

                    fieldIndex++;
                }

                fieldStart = -1;
                escaped = false;
                continue;
            }

            if (fieldStart < 0) {
                fieldStart = i;
            }

            escaped = false;
        }

        return fieldIndex >= 3 && !mountPoint.IsEmpty && !fileSystemType.IsEmpty;
    }

    private static bool TryDecodeUtf8(ReadOnlySpan<byte> source, Span<char> destination, out int charsWritten) {
        int charCount = Encoding.UTF8.GetCharCount(source);
        if (charCount > destination.Length) {
            charsWritten = 0;
            return false;
        }

        charsWritten = Encoding.UTF8.GetChars(source, destination);
        return true;
    }
}
