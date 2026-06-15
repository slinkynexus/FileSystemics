using System.Buffers;
using System.Text;

namespace FileSystemics.IO.Internal;

/// <summary>
/// Encodes path spans into null-terminated platform buffers (UTF-16 on Windows, UTF-8 on Unix).
/// </summary>
internal static class PlatformPathBuffer {
    internal const int STACK_THRESHOLD_CHARS = 512;

    internal static int Utf16Capacity(ReadOnlySpan<char> path) => path.Length + 1;

    internal static int Utf8Capacity(ReadOnlySpan<char> path) => Encoding.UTF8.GetByteCount(path) + 1;

    internal static int Utf16CombinedCapacity(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName, char separator) {
        bool needsSeparator = NeedsSeparator(directory, separator);
        return directory.Length + (needsSeparator ? 1 : 0) + fileName.Length + 1;
    }

    internal static int Utf8CombinedCapacity(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName, char separator) {
        bool needsSeparator = NeedsSeparator(directory, separator);
        int byteCount = Encoding.UTF8.GetByteCount(directory) + Encoding.UTF8.GetByteCount(fileName);
        if (needsSeparator) {
            byteCount += Encoding.UTF8.GetByteCount([separator]);
        }

        return byteCount + 1;
    }

    internal static void EncodeUtf16(ReadOnlySpan<char> path, Span<char> destination) {
        ThrowIfContainsNull(path, nameof(path));
        path.CopyTo(destination);
        destination[path.Length] = '\0';
    }

    internal static void EncodeUtf8(ReadOnlySpan<char> path, Span<byte> destination) {
        ThrowIfContainsNull(path, nameof(path));
        int byteCount = Encoding.UTF8.GetByteCount(path);
        Encoding.UTF8.GetBytes(path, destination);
        destination[byteCount] = 0;
    }

    internal static void EncodeUtf16Combined(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Span<char> destination) {
        ThrowIfContainsNull(directory, fileName);
        directory.CopyTo(destination);
        int offset = directory.Length;
        if (NeedsSeparator(directory, separator)) {
            destination[offset++] = separator;
        }

        fileName.CopyTo(destination[offset..]);
        int charCount = offset + fileName.Length;
        destination[charCount] = '\0';
    }

    internal static void EncodeUtf8Combined(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Span<byte> destination) {
        ThrowIfContainsNull(directory, fileName);
        int written = Encoding.UTF8.GetBytes(directory, destination);
        if (NeedsSeparator(directory, separator)) {
            written += Encoding.UTF8.GetBytes([separator], destination[written..]);
        }

        written += Encoding.UTF8.GetBytes(fileName, destination[written..]);
        destination[written] = 0;
    }

    internal static bool TryCreate(
        ReadOnlySpan<char> path,
        bool useUtf16,
        scoped ref PlatformPathBufferRental buffer,
        out int encodedLength) {
        if (useUtf16) {
            encodedLength = path.Length;
            int capacity = Utf16Capacity(path);
            Span<char> destination = buffer.AllocateChars(capacity);
            EncodeUtf16(path, destination);
            return true;
        }

        encodedLength = Encoding.UTF8.GetByteCount(path);
        int byteCapacity = encodedLength + 1;
        Span<byte> bytes = buffer.AllocateBytes(byteCapacity);
        EncodeUtf8(path, bytes);
        return true;
    }

    internal static bool TryCreate(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        bool useUtf16,
        scoped ref PlatformPathBufferRental buffer,
        out int encodedLength) {
        bool needsSeparator = NeedsSeparator(directory, separator);
        int charCount = directory.Length + (needsSeparator ? 1 : 0) + fileName.Length;
        if (useUtf16) {
            encodedLength = charCount;
            int capacity = charCount + 1;
            Span<char> destination = buffer.AllocateChars(capacity);
            EncodeUtf16Combined(directory, fileName, separator, destination);
            return true;
        }

        int byteCount = Encoding.UTF8.GetByteCount(directory) +
            (needsSeparator ? Encoding.UTF8.GetByteCount([separator]) : 0) +
            Encoding.UTF8.GetByteCount(fileName);
        encodedLength = byteCount;
        int byteCapacity = byteCount + 1;
        Span<byte> bytes = buffer.AllocateBytes(byteCapacity);
        EncodeUtf8Combined(directory, fileName, separator, bytes);
        return true;
    }

    private static bool NeedsSeparator(ReadOnlySpan<char> directory, char separator) =>
        directory.Length > 0 &&
        directory[^1] != separator &&
        directory[^1] != '\\' &&
        directory[^1] != '/';

    private static void ThrowIfContainsNull(ReadOnlySpan<char> path, string paramName) =>
        PathArgumentValidation.ValidatePath(path, paramName);

    private static void ThrowIfContainsNull(ReadOnlySpan<char> directory, ReadOnlySpan<char> fileName) {
        if (!directory.IsEmpty) {
            PathArgumentValidation.ValidatePath(directory, nameof(directory));
        }

        PathArgumentValidation.ValidatePath(fileName, nameof(fileName));
    }
}

internal ref struct PlatformPathBufferRental {
    private char[]? _charPool;
    private byte[]? _bytePool;
    private int _charLength;
    private int _byteLength;

    internal Span<char> AllocateChars(int capacity) {
        _charPool = ArrayPool<char>.Shared.Rent(capacity);
        _charLength = capacity;
        return _charPool.AsSpan(0, capacity);
    }

    internal Span<byte> AllocateBytes(int capacity) {
        _bytePool = ArrayPool<byte>.Shared.Rent(capacity);
        _byteLength = capacity;
        return _bytePool.AsSpan(0, capacity);
    }

    public ReadOnlySpan<char> AsUtf16() {
        if (_charPool is not null) {
            return _charPool.AsSpan(0, _charLength);
        }

        throw new InvalidOperationException("Buffer does not contain UTF-16 data.");
    }

    public ReadOnlySpan<byte> AsUtf8() {
        if (_bytePool is not null) {
            return _bytePool.AsSpan(0, _byteLength);
        }

        throw new InvalidOperationException("Buffer does not contain UTF-8 data.");
    }

    public void Dispose() {
        if (_charPool is not null) {
            ArrayPool<char>.Shared.Return(_charPool);
            _charPool = null;
        }

        if (_bytePool is not null) {
            ArrayPool<byte>.Shared.Return(_bytePool);
            _bytePool = null;
        }
    }
}

/// <summary>
/// Stack-first path buffer helper; rents from array pool when paths exceed <see cref="PlatformPathBuffer.STACK_THRESHOLD_CHARS"/>.
/// </summary>
internal static class PlatformPath {
    public static T WithPath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> windows,
        Func<ReadOnlySpan<byte>, T> unix) =>
        WithNativePath(path, (utf16, utf8) => utf8.IsEmpty ? windows(utf16, utf8) : unix(utf8));

    public static T WithCombinedPath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> windows,
        Func<ReadOnlySpan<byte>, T> unix) =>
        WithCombinedNativePath(directory, fileName, separator, (utf16, utf8) =>
            utf8.IsEmpty ? windows(utf16, utf8) : unix(utf8));

    public static T WithTwoPaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> windows,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> unix) =>
        WithTwoNativePaths(path1, path2, windows, unix);

    public static T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
        NativePlatformTable.PathEncoding.WithNativePath(path, action);

    public static T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action) =>
        NativePlatformTable.PathEncoding.WithCombinedNativePath(directory, fileName, separator, action);

    public static T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action) =>
        NativePlatformTable.PathEncoding.WithTwoNativePaths(path1, path2, utf16Action, utf8Action);
}
