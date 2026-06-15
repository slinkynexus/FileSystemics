namespace FileSystemics.IO.Internal;

internal sealed class NativePlatformPathEncoding : INativePathEncoding {
    internal static readonly NativePlatformPathEncoding Instance = new();

    private NativePlatformPathEncoding() {
    }

    public T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) {
        if (NativePlatformTable.UsesUtf16NativePaths) {
            int capacity = PlatformPathBuffer.Utf16Capacity(path);
            if (capacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS) {
                Span<char> buffer = stackalloc char[capacity];
                PlatformPathBuffer.EncodeUtf16(path, buffer);
                return action(buffer, ReadOnlySpan<byte>.Empty);
            }

            PlatformPathBufferRental rented = default;
            try {
                PlatformPathBuffer.TryCreate(path, useUtf16: true, ref rented, out _);
                return action(rented.AsUtf16(), ReadOnlySpan<byte>.Empty);
            }
            finally {
                rented.Dispose();
            }
        }

        return PosixPathEncoding.WithNativePath(path, action);
    }

    public T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) {
        if (NativePlatformTable.UsesUtf16NativePaths) {
            int capacity = PlatformPathBuffer.Utf16CombinedCapacity(directory, fileName, separator);
            if (capacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS) {
                Span<char> buffer = stackalloc char[capacity];
                PlatformPathBuffer.EncodeUtf16Combined(directory, fileName, separator, buffer);
                return action(buffer, ReadOnlySpan<byte>.Empty);
            }

            PlatformPathBufferRental rented = default;
            try {
                PlatformPathBuffer.TryCreate(directory, fileName, separator, useUtf16: true, ref rented, out _);
                return action(rented.AsUtf16(), ReadOnlySpan<byte>.Empty);
            }
            finally {
                rented.Dispose();
            }
        }

        return PosixPathEncoding.WithCombinedNativePath(directory, fileName, separator, action);
    }

    public T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) {
        if (NativePlatformTable.UsesUtf16NativePaths) {
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

        return PosixPathEncoding.WithTwoNativePaths(path1, path2, utf16Action, utf8Action);
    }
}
