namespace FileSystemics.IO.Internal;

internal sealed class WindowsPathRules : INativePathRules<WindowsPathRules> {
    private WindowsPathRules() {
    }

    public static char DirectorySeparatorChar => '\\';

    public static char AltDirectorySeparatorChar => '/';

    public static char VolumeSeparatorChar => ':';

    public static char PathSeparator => ';';

    public static StringComparison PathComparison => StringComparison.OrdinalIgnoreCase;

    public static bool UsesUtf16NativePaths => true;

    public static bool UsesGetdents64DirectoryEnumeration => false;

    public static int GetRootLength(ReadOnlySpan<char> path) {
        if (path.IsEmpty) {
            return 0;
        }

        int length = path.Length;
        if (length >= 1 && IsDirectorySeparator(path[0])) {
            if (length >= 2 && IsDirectorySeparator(path[1])) {
                int i = 2;
                int segments = 0;
                while (i < length && segments < 2) {
                    if (IsDirectorySeparator(path[i])) {
                        segments++;
                        if (segments == 1) {
                            i++;
                            continue;
                        }
                        break;
                    }
                    i++;
                }
                return i;
            }
            return 1;
        }

        if (length >= 2 && path[1] == VolumeSeparatorChar && IsValidDriveChar(path[0])) {
            return length >= 3 && IsDirectorySeparator(path[2]) ? 3 : 2;
        }

        return 0;
    }

    public static bool IsPathRooted(ReadOnlySpan<char> path) {
        if (path.IsEmpty) {
            return false;
        }

        if (IsDirectorySeparator(path[0])) {
            return true;
        }

        return path.Length >= 2 && IsValidDriveChar(path[0]) && path[1] == VolumeSeparatorChar;
    }

    public static bool IsPartiallyQualified(ReadOnlySpan<char> path) {
        if (path.IsEmpty) {
            return true;
        }

        if (path.Length < 2) {
            return true;
        }

        if (path[1] == VolumeSeparatorChar) {
            return !IsValidDriveChar(path[0]) || (path.Length != 2 && !IsDirectorySeparator(path[2]));
        }

        if (IsDirectorySeparator(path[0])) {
            if (path.Length >= 4 &&
                path[0] == '\\' &&
                path[1] == '\\' &&
                path[2] == '?' &&
                path[3] == '\\') {
                return false;
            }

            if (path.Length >= 2 && IsDirectorySeparator(path[1])) {
                return false;
            }

            return true;
        }

        return GetRootLength(path) <= 0;
    }

    public static bool IsDirectorySeparator(char value) =>
        value == DirectorySeparatorChar || value == AltDirectorySeparatorChar;

    public static bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path) {
        if (path.IsEmpty || path.Contains('\0')) {
            return true;
        }

        return IsEffectivelyEmpty(path);
    }

    public static bool IsEffectivelyEmpty(ReadOnlySpan<char> path) {
        if (path.IsEmpty) {
            return true;
        }

        foreach (char value in path) {
            if (value != ' ') {
                return false;
            }
        }

        return true;
    }

    public static T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) {
        int capacity = WindowsNativePathEncoding.GetCapacity(path);
        if (capacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS) {
            Span<char> buffer = stackalloc char[capacity];
            WindowsNativePathEncoding.Encode(path, buffer, out _);
            return action(buffer, ReadOnlySpan<byte>.Empty);
        }

        PlatformPathBufferRental rented = default;
        try {
            Span<char> buffer = rented.AllocateChars(capacity);
            WindowsNativePathEncoding.Encode(path, buffer, out _);
            return action(buffer, ReadOnlySpan<byte>.Empty);
        }
        finally {
            rented.Dispose();
        }
    }

    public static T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) {
        int joinedCapacity = PlatformPathBuffer.Utf16CombinedCapacity(directory, fileName, separator);
        Span<char> joined = joinedCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS
            ? stackalloc char[joinedCapacity]
            : new char[joinedCapacity];
        PlatformPathBuffer.EncodeUtf16Combined(directory, fileName, separator, joined);
        ReadOnlySpan<char> joinedPath = joined[..^1];
        int capacity = WindowsNativePathEncoding.GetCapacity(joinedPath);
        if (capacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS) {
            Span<char> buffer = stackalloc char[capacity];
            WindowsNativePathEncoding.Encode(joinedPath, buffer, out _);
            return action(buffer, ReadOnlySpan<byte>.Empty);
        }

        PlatformPathBufferRental rented = default;
        try {
            Span<char> buffer = rented.AllocateChars(capacity);
            WindowsNativePathEncoding.Encode(joinedPath, buffer, out _);
            return action(buffer, ReadOnlySpan<byte>.Empty);
        }
        finally {
            rented.Dispose();
        }
    }

    public static T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) {
        PathArgumentValidation.ValidatePath(path1, nameof(path1));
        PathArgumentValidation.ValidatePath(path2, nameof(path2));

        int capacity1 = PlatformPathBuffer.Utf16Capacity(path1);
        int capacity2 = PlatformPathBuffer.Utf16Capacity(path2);
        if (capacity1 <= PlatformPathBuffer.STACK_THRESHOLD_CHARS &&
            capacity2 <= PlatformPathBuffer.STACK_THRESHOLD_CHARS) {
            Span<char> buffer1 = stackalloc char[capacity1];
            Span<char> buffer2 = stackalloc char[capacity2];
            PlatformPathBuffer.EncodeUtf16(path1, buffer1);
            PlatformPathBuffer.EncodeUtf16(path2, buffer2);
            return utf16Action(buffer1, buffer2);
        }

        PlatformPathBufferRental rented1 = default;
        PlatformPathBufferRental rented2 = default;
        try {
            PlatformPathBuffer.TryCreate(path1, useUtf16: true, ref rented1, out _);
            PlatformPathBuffer.TryCreate(path2, useUtf16: true, ref rented2, out _);
            return utf16Action(rented1.AsUtf16(), rented2.AsUtf16());
        }
        finally {
            rented1.Dispose();
            rented2.Dispose();
        }
    }

    private static bool IsValidDriveChar(char value) =>
        (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
}
