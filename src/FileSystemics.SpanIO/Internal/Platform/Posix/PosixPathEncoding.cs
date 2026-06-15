namespace FileSystemics.IO.Internal;

internal static class PosixPathEncoding {
    internal static T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) {
        int byteCapacity = PlatformPathBuffer.Utf8Capacity(path);
        if (byteCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS * 4) {
            Span<byte> buffer = stackalloc byte[byteCapacity];
            PlatformPathBuffer.EncodeUtf8(path, buffer);
            return action(ReadOnlySpan<char>.Empty, buffer);
        }

        PlatformPathBufferRental rentedBytes = default;
        try {
            PlatformPathBuffer.TryCreate(path, useUtf16: false, ref rentedBytes, out _);
            return action(ReadOnlySpan<char>.Empty, rentedBytes.AsUtf8());
        }
        finally {
            rentedBytes.Dispose();
        }
    }

    internal static T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) {
        int byteCapacity = PlatformPathBuffer.Utf8CombinedCapacity(directory, fileName, separator);
        if (byteCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS * 4) {
            Span<byte> buffer = stackalloc byte[byteCapacity];
            PlatformPathBuffer.EncodeUtf8Combined(directory, fileName, separator, buffer);
            return action(ReadOnlySpan<char>.Empty, buffer);
        }

        PlatformPathBufferRental rentedBytes = default;
        try {
            PlatformPathBuffer.TryCreate(directory, fileName, separator, useUtf16: false, ref rentedBytes, out _);
            return action(ReadOnlySpan<char>.Empty, rentedBytes.AsUtf8());
        }
        finally {
            rentedBytes.Dispose();
        }
    }

    internal static T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) {
        PathArgumentValidation.ValidatePath(path1, nameof(path1));
        PathArgumentValidation.ValidatePath(path2, nameof(path2));

        int byteCapacity1 = PlatformPathBuffer.Utf8Capacity(path1);
        int byteCapacity2 = PlatformPathBuffer.Utf8Capacity(path2);
        if (byteCapacity1 + byteCapacity2 <= PlatformPathBuffer.STACK_THRESHOLD_CHARS * 4) {
            Span<byte> buffer1 = stackalloc byte[byteCapacity1];
            Span<byte> buffer2 = stackalloc byte[byteCapacity2];
            PlatformPathBuffer.EncodeUtf8(path1, buffer1);
            PlatformPathBuffer.EncodeUtf8(path2, buffer2);
            return utf8Action(buffer1, buffer2);
        }

        PlatformPathBufferRental rented1 = default;
        PlatformPathBufferRental rented2 = default;
        try {
            PlatformPathBuffer.TryCreate(path1, useUtf16: false, ref rented1, out _);
            PlatformPathBuffer.TryCreate(path2, useUtf16: false, ref rented2, out _);
            return utf8Action(rented1.AsUtf8(), rented2.AsUtf8());
        }
        finally {
            rented1.Dispose();
            rented2.Dispose();
        }
    }
}
