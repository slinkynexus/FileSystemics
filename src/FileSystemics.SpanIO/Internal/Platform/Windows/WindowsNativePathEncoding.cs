using FileSystemics.IO.Interop;

namespace FileSystemics.IO.Internal;

/// <summary>
/// Applies the <c>\\?\</c> extended-length prefix required by Win32 for paths at or above <see cref="InteropWindowsDrive.MAX_PATH"/>.
/// </summary>
internal static class WindowsNativePathEncoding {
    private static ReadOnlySpan<char> ExtendedPrefix => @"\\?\".AsSpan();
    private static ReadOnlySpan<char> UncExtendedPrefix => @"\\?\UNC\".AsSpan();

    internal static int GetCapacity(ReadOnlySpan<char> path) {
        int prefixLength = GetPrefixLength(path);
        return path.Length + prefixLength + 1;
    }

    internal static void Encode(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) {
        PathArgumentValidation.ValidatePath(path, nameof(path));
        int offset = 0;
        if (NeedsExtendedPrefix(path)) {
            if (IsUnc(path)) {
                UncExtendedPrefix.CopyTo(destination);
                offset = UncExtendedPrefix.Length;
                path[2..].CopyTo(destination[offset..]);
                offset += path.Length - 2;
            }
            else {
                ExtendedPrefix.CopyTo(destination);
                offset = ExtendedPrefix.Length;
                path.CopyTo(destination[offset..]);
                offset += path.Length;
            }
        }
        else {
            path.CopyTo(destination);
            offset = path.Length;
        }

        destination[offset] = '\0';
        charsWritten = offset;
    }

    private static int GetPrefixLength(ReadOnlySpan<char> path) {
        if (!NeedsExtendedPrefix(path)) {
            return 0;
        }

        return IsUnc(path) ? UncExtendedPrefix.Length - 2 : ExtendedPrefix.Length;
    }

    private static bool NeedsExtendedPrefix(ReadOnlySpan<char> path) =>
        path.Length >= InteropWindowsDrive.MAX_PATH && !IsExtended(path);

    private static bool IsExtended(ReadOnlySpan<char> path) =>
        path.Length >= 4 &&
        path[0] == '\\' &&
        path[1] == '\\' &&
        path[2] == '?' &&
        path[3] == '\\';

    private static bool IsUnc(ReadOnlySpan<char> path) =>
        path.Length >= 2 && path[0] == '\\' && path[1] == '\\' && !IsExtended(path);
}
