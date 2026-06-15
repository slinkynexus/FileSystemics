using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

/// <summary>
/// Span-based path manipulation mirroring <see cref="Path"/> operations.
/// </summary>
public static partial class SpanPath {
    /// <summary>Character separating directory levels.</summary>
    public static char DirectorySeparatorChar => NativePlatformTable.DirectorySeparatorChar;

    /// <summary>Alternate character separating directory levels.</summary>
    public static char AltDirectorySeparatorChar => NativePlatformTable.AltDirectorySeparatorChar;

    /// <summary>Character separating volume from directory.</summary>
    public static char VolumeSeparatorChar => NativePlatformTable.VolumeSeparatorChar;

    /// <summary>Character separating paths in environment variables.</summary>
    public static char PathSeparator => NativePlatformTable.PathSeparator;

    /// <summary>Returns the file name and extension of a path.</summary>
    public static ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path) {
        int rootLength = GetPathRoot(path).Length;
        int index = DirectorySeparatorChar == AltDirectorySeparatorChar
            ? path.LastIndexOf(DirectorySeparatorChar)
            : path.LastIndexOfAny(DirectorySeparatorChar, AltDirectorySeparatorChar);
        int start = index < rootLength ? rootLength : index + 1;
        return path[start..];
    }

    /// <summary>Returns the directory information for a path.</summary>
    public static ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path) {
        if (path.IsEmpty) {
            return ReadOnlySpan<char>.Empty;
        }

        int end = GetDirectoryNameOffset(path);
        ReadOnlySpan<char> result = end >= 0 ? path[..end] : ReadOnlySpan<char>.Empty;
        return CanonicalizeWindowsPathSeparators(result);
    }

    /// <summary>Returns the extension of a path.</summary>
    public static ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path) {
        for (int i = path.Length - 1; i >= 0; i--) {
            char ch = path[i];
            if (ch == '.') {
                return i != path.Length - 1 ? path[i..] : ReadOnlySpan<char>.Empty;
            }

            if (IsDirectorySeparator(ch)) {
                break;
            }
        }

        return ReadOnlySpan<char>.Empty;
    }

    /// <summary>Returns the file name without extension.</summary>
    public static ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path) {
        ReadOnlySpan<char> fileName = GetFileName(path);
        int lastPeriod = fileName.LastIndexOf('.');
        return lastPeriod < 0 ? fileName : fileName[..lastPeriod];
    }

    /// <summary>Returns the root portion of a path.</summary>
    public static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path) {
        int rootLength = GetRootLength(path);
        ReadOnlySpan<char> result = rootLength == 0 ? ReadOnlySpan<char>.Empty : path[..rootLength];
        return CanonicalizeWindowsPathSeparators(result);
    }

    /// <summary>Determines whether a path includes a file extension.</summary>
    public static bool HasExtension(ReadOnlySpan<char> path) => !GetExtension(path).IsEmpty;

    /// <summary>Determines whether a path includes a root.</summary>
    public static bool IsPathRooted(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeIsPathRooted(path);

    /// <summary>Determines whether a path is fully qualified.</summary>
    public static bool IsPathFullyQualified(ReadOnlySpan<char> path) =>
        !NativePlatformTable.InvokeIsPartiallyQualified(path);

    /// <summary>Determines whether a path ends with a directory separator.</summary>
    public static bool EndsInDirectorySeparator(ReadOnlySpan<char> path) =>
        path.Length > 0 && IsDirectorySeparator(path[^1]);

    /// <summary>Removes trailing directory separators when safe.</summary>
    public static ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path) =>
        EndsInDirectorySeparator(path) && !IsRoot(path) ? path[..^1] : path;

    internal static int GetJoinLength(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2) {
        if (path1.Length == 0) return path2.Length;
        if (path2.Length == 0) return path1.Length;

        bool needsSeparator = !(EndsInDirectorySeparator(path1) || StartsWithDirectorySeparator(path2));
        return path1.Length + path2.Length + (needsSeparator ? 1 : 0);
    }

    private static int GetJoinLength(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3) {
        if (path1.Length == 0) return GetJoinLength(path2, path3);
        if (path2.Length == 0) return GetJoinLength(path1, path3);
        if (path3.Length == 0) return GetJoinLength(path1, path2);

        int neededSeparators = EndsInDirectorySeparator(path1) || StartsWithDirectorySeparator(path2) ? 0 : 1;
        if (!(EndsInDirectorySeparator(path2) || StartsWithDirectorySeparator(path3))) {
            neededSeparators++;
        }

        return path1.Length + path2.Length + path3.Length + neededSeparators;
    }

    /// <summary>Joins two paths into a destination buffer.</summary>
    public static bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination,
        out int charsWritten) {
        charsWritten = 0;
        if (path1.Length == 0 && path2.Length == 0) {
            return true;
        }

        if (path1.Length == 0 || path2.Length == 0) {
            ReadOnlySpan<char> pathToUse = path1.Length == 0 ? path2 : path1;
            if (destination.Length < pathToUse.Length) {
                return false;
            }

            pathToUse.CopyTo(destination);
            charsWritten = pathToUse.Length;
            return true;
        }

        bool needsSeparator = !(EndsInDirectorySeparator(path1) || StartsWithDirectorySeparator(path2));
        int charsNeeded = path1.Length + path2.Length + (needsSeparator ? 1 : 0);
        if (destination.Length < charsNeeded) {
            return false;
        }

        path1.CopyTo(destination);
        int offset = path1.Length;
        if (needsSeparator) {
            destination[offset++] = DirectorySeparatorChar;
        }

        path2.CopyTo(destination[offset..]);
        charsWritten = charsNeeded;
        return true;
    }

    /// <summary>Joins two paths, throwing when the destination is too small.</summary>
    public static ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination) =>
        TryJoin(path1, path2, destination, out int charsWritten)
            ? destination[..charsWritten]
            : throw SpanIOException.DestinationTooSmall();


    /// <summary>Joins three paths into a destination buffer.</summary>
    public static bool TryJoin(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        ReadOnlySpan<char> path3,
        Span<char> destination,
        out int charsWritten) {
        charsWritten = 0;
        if (path1.Length == 0) {
            return TryJoin(path2, path3, destination, out charsWritten);
        }

        if (path2.Length == 0) {
            return TryJoin(path1, path3, destination, out charsWritten);
        }

        if (path3.Length == 0) {
            return TryJoin(path1, path2, destination, out charsWritten);
        }

        int charsNeeded = GetJoinLength(path1, path2, path3);
        if (destination.Length < charsNeeded) {
            return false;
        }

        if (!TryJoin(path1, path2, destination, out charsWritten)) {
            return false;
        }

        bool needsSeparator = !(EndsInDirectorySeparator(path2) || StartsWithDirectorySeparator(path3));
        if (needsSeparator) {
            destination[charsWritten++] = DirectorySeparatorChar;
        }

        path3.CopyTo(destination[charsWritten..]);
        charsWritten += path3.Length;
        return true;
    }

    /// <summary>Joins three paths, throwing when the destination is too small.</summary>
    public static ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination) =>
        TryJoin(path1, path2, path3, destination, out int charsWritten)
            ? destination[..charsWritten]
            : throw SpanIOException.DestinationTooSmall();


    /// <summary>Combines two paths into a destination buffer.</summary>
    public static bool TryCombine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten) {
        if (path2.Length == 0 || !IsPathRooted(path2)) {
            return TryJoin(path1, path2, destination, out charsWritten);
        }

        if (destination.Length < path2.Length) {
            charsWritten = 0;
            return false;
        }

        path2.CopyTo(destination);
        charsWritten = path2.Length;
        return true;

    }

    /// <summary>Combines two paths, throwing when the destination is too small.</summary>
    public static ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination) =>
        TryCombine(path1, path2, destination, out int charsWritten)
            ? destination[..charsWritten]
            : throw SpanIOException.DestinationTooSmall();

    /// <summary>Combines three paths into a destination buffer.</summary>
    public static bool TryCombine(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        ReadOnlySpan<char> path3,
        Span<char> destination,
        out int charsWritten) {
        if (path3.Length != 0 && IsPathRooted(path3)) {
            if (destination.Length < path3.Length) {
                charsWritten = 0;
                return false;
            }

            path3.CopyTo(destination);
            charsWritten = path3.Length;
            return true;
        }

        if (path2.Length != 0 && IsPathRooted(path2)) {
            return TryJoin(path2, path3, destination, out charsWritten);
        }

        return TryJoin(path1, path2, path3, destination, out charsWritten);
    }

    /// <summary>Combines three paths, throwing when the destination is too small.</summary>
    public static ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination) =>
        TryCombine(path1, path2, path3, destination, out int charsWritten)
            ? destination[..charsWritten]
            : throw SpanIOException.DestinationTooSmall();

    /// <summary>Changes the extension of a path in a destination buffer.</summary>
    public static bool TryChangeExtension(
        ReadOnlySpan<char> path,
        ReadOnlySpan<char> extension,
        Span<char> destination,
        out int charsWritten) {
        if (path.IsEmpty) {
            charsWritten = 0;
            return true;
        }

        int subLength = path.Length;
        for (int i = path.Length - 1; i >= 0; i--) {
            char ch = path[i];
            if (ch == '.') {
                subLength = i;
                break;
            }

            if (IsDirectorySeparator(ch)) {
                break;
            }
        }

        ReadOnlySpan<char> basePath = path[..subLength];
        if (extension.IsEmpty) {
            int emptyExtensionLength = basePath.Length + 1;
            if (destination.Length < emptyExtensionLength) {
                charsWritten = 0;
                return false;
            }

            basePath.CopyTo(destination);
            destination[basePath.Length] = '.';
            charsWritten = emptyExtensionLength;
            return true;
        }

        ReadOnlySpan<char> dottedExtension = extension.StartsWith('.') ? extension : default;
        int needed = basePath.Length + (dottedExtension.IsEmpty ? 1 + extension.Length : extension.Length);
        if (destination.Length < needed) {
            charsWritten = 0;
            return false;
        }

        basePath.CopyTo(destination);
        int offset = basePath.Length;
        if (dottedExtension.IsEmpty) {
            destination[offset++] = '.';
        }

        extension.CopyTo(destination[offset..]);
        charsWritten = needed;
        return true;
    }

    /// <summary>Changes the extension of a path, throwing when the destination is too small.</summary>
    public static ReadOnlySpan<char> ChangeExtension(ReadOnlySpan<char> path, ReadOnlySpan<char> extension, Span<char> destination) =>
        TryChangeExtension(path, extension, destination, out int charsWritten)
            ? destination[..charsWritten]
            : throw SpanIOException.DestinationTooSmall();

    /// <summary>Gets a relative path into a destination buffer.</summary>
    public static bool TryGetRelativePath(
        ReadOnlySpan<char> relativeTo,
        ReadOnlySpan<char> path,
        Span<char> destination,
        out int charsWritten) {

        charsWritten = 0;
        if (relativeTo.IsEmpty || path.IsEmpty) {
            return false;
        }

        ReadOnlySpan<char> currentDirectory = Environment.CurrentDirectory.AsSpan();
        bool relativeToIsFullyQualified = IsPathFullyQualified(relativeTo);
        bool relativeToRequiresNormalization = SpanPathFullPath.RequiresFullNormalization(relativeTo);
        int pathCapacity = SpanPathFullPath.EstimateBufferCapacity(path, currentDirectory);
        if (relativeToIsFullyQualified && !relativeToRequiresNormalization && pathCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS) {
            Span<char> pathBuffer = stackalloc char[pathCapacity];
            if (!SpanPathFullPath.TryGetFullPath(path, pathBuffer, currentDirectory, out int normalizedPathLength)) {
                return false;
            }

            return TryGetRelativePathFromNormalized(
                relativeTo,
                pathBuffer[..normalizedPathLength],
                destination,
                out charsWritten);
        }

        int relativeToCapacity = SpanPathFullPath.EstimateBufferCapacity(relativeTo, currentDirectory);
        if (relativeToCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS &&
            pathCapacity <= PlatformPathBuffer.STACK_THRESHOLD_CHARS) {
            Span<char> relativeToBuffer = stackalloc char[relativeToCapacity];
            Span<char> pathBuffer = stackalloc char[pathCapacity];
            if (!SpanPathFullPath.TryGetFullPath(relativeTo, relativeToBuffer, currentDirectory, out int normalizedRelativeToLength) ||
                !SpanPathFullPath.TryGetFullPath(path, pathBuffer, currentDirectory, out int normalizedPathLength)) {
                return false;
            }

            return TryGetRelativePathFromNormalized(
                relativeToBuffer[..normalizedRelativeToLength],
                pathBuffer[..normalizedPathLength],
                destination,
                out charsWritten);
        }

        char[] pathArray = GC.AllocateUninitializedArray<char>(pathCapacity);
        if (!SpanPathFullPath.TryGetFullPath(path, pathArray, currentDirectory, out int heapPathLength)) {
            return false;
        }

        ReadOnlySpan<char> normalizedPath = pathArray.AsSpan(0, heapPathLength);
        if (relativeToIsFullyQualified && !relativeToRequiresNormalization) {
            return TryGetRelativePathFromNormalized(relativeTo, normalizedPath, destination, out charsWritten);
        }

        char[] relativeToArray = GC.AllocateUninitializedArray<char>(relativeToCapacity);
        if (!SpanPathFullPath.TryGetFullPath(relativeTo, relativeToArray, currentDirectory, out int heapRelativeToLength)) {
            return false;
        }

        return TryGetRelativePathFromNormalized(
            relativeToArray.AsSpan(0, heapRelativeToLength),
            normalizedPath,
            destination,
            out charsWritten);
    }

    private static bool TryGetRelativePathFromNormalized(
        ReadOnlySpan<char> relativeTo,
        ReadOnlySpan<char> path,
        Span<char> destination,
        out int charsWritten) {
        charsWritten = 0;
        StringComparison comparison = PathComparison;
        if (!AreRootsEqual(relativeTo, path, comparison)) {
            if (destination.Length < path.Length) {
                return false;
            }

            path.CopyTo(destination);
            charsWritten = path.Length;
            return true;
        }

        int commonLength = GetCommonPathLength(relativeTo, path,
            ignoreCase: comparison == StringComparison.OrdinalIgnoreCase);
        if (commonLength == 0) {
            if (destination.Length < path.Length) {
                return false;
            }

            path.CopyTo(destination);
            charsWritten = path.Length;
            return true;
        }

        int relativeToLength = relativeTo.Length;
        if (EndsInDirectorySeparator(relativeTo)) {
            relativeToLength--;
        }

        bool pathEndsInSeparator = EndsInDirectorySeparator(path);
        int pathLength = path.Length;
        if (pathEndsInSeparator) {
            pathLength--;
        }

        if (relativeToLength == pathLength && commonLength >= relativeToLength) {
            if (destination.Length < 1) {
                return false;
            }

            destination[0] = '.';
            charsWritten = 1;
            return true;
        }

        SpanBuilder builder = new(destination);
        ReadOnlySpan<char> twoDots = ['.', '.'];
        if (commonLength < relativeToLength) {
            if (!builder.TryAppend(twoDots)) {
                return false;
            }

            for (int i = commonLength + 1; i < relativeToLength; i++) {
                if (IsDirectorySeparator(relativeTo[i])) {
                    if (!builder.TryAppend(DirectorySeparatorChar) ||
                        !builder.TryAppend(twoDots)) {
                        return false;
                    }
                }
            }
        } else if (IsDirectorySeparator(path[commonLength])) {
            commonLength++;
        }

        int differenceLength = pathLength - commonLength;
        if (pathEndsInSeparator) {
            differenceLength++;
        }

        if (differenceLength > 0) {
            if (builder.Length > 0 && !builder.TryAppend(DirectorySeparatorChar)) {
                return false;
            }

            if (!builder.TryAppend(path.Slice(commonLength, differenceLength))) {
                return false;
            }
        }

        charsWritten = builder.Length;
        return true;
    }

    /// <summary>Gets a relative path, throwing when the destination is too small.</summary>
    public static ReadOnlySpan<char> GetRelativePath(ReadOnlySpan<char> relativeTo, ReadOnlySpan<char> path, Span<char> destination) {
        if (relativeTo.IsEmpty) {
            throw SpanIOException.EmptyPath(nameof(relativeTo));
        }

        if (path.IsEmpty) {
            throw SpanIOException.EmptyPath(nameof(path));
        }

        return TryGetRelativePath(relativeTo, path, destination, out int charsWritten)
            ? destination[..charsWritten]
            : throw SpanIOException.DestinationTooSmall();
    }

    private static StringComparison PathComparison => NativePlatformTable.PathComparison;

    private static bool StartsWithDirectorySeparator(ReadOnlySpan<char> path) =>
        path.Length > 0 && IsDirectorySeparator(path[0]);

    private static bool IsDirectorySeparator(char c) {
        char separator = DirectorySeparatorChar;
        char altSeparator = AltDirectorySeparatorChar;
        return c == separator || (altSeparator != separator && c == altSeparator);
    }

    private static int GetRootLength(ReadOnlySpan<char> path) {
        if (!NativePlatformTable.UsesUtf16NativePaths) {
            return path.IsEmpty || path[0] != DirectorySeparatorChar ? 0 : 1;
        }

        return NativePlatformTable.InvokeGetRootLength(path);
    }

    private static bool IsRoot(ReadOnlySpan<char> path) =>
        path.Length == GetRootLength(path);

    private static int GetDirectoryNameOffset(ReadOnlySpan<char> path) {
        int rootLength = GetRootLength(path);
        if (path.Length <= rootLength) {
            return -1;
        }

        ReadOnlySpan<char> fileName = GetFileName(path);
        if (fileName.Length == path.Length) {
            return 0;
        }

        int end = path.Length - fileName.Length;
        char separator = DirectorySeparatorChar;
        char altSeparator = AltDirectorySeparatorChar;
        while (end > rootLength) {
            char ch = path[end - 1];
            if (ch != separator && (altSeparator == separator || ch != altSeparator)) {
                break;
            }

            end--;
        }

        return end;
    }

    private static int GetCommonPathLength(ReadOnlySpan<char> first, ReadOnlySpan<char> second, bool ignoreCase) {
        int commonChars = EqualStartingCharacterCount(first, second, ignoreCase);
        if (commonChars == 0 ||
            (commonChars == first.Length && (commonChars == second.Length || IsDirectorySeparator(second[commonChars]))) ||
            (commonChars == second.Length && IsDirectorySeparator(first[commonChars]))) {
            return commonChars;
        }

        while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1])) {
            commonChars--;
        }

        return commonChars;
    }

    private static int EqualStartingCharacterCount(ReadOnlySpan<char> first, ReadOnlySpan<char> second, bool ignoreCase) {
        if (first.Length == 0 || second.Length == 0) {
            return 0;
        }

        int commonLength = first.CommonPrefixLength(second);
        if (!ignoreCase) {
            return commonLength;
        }

        for (; (uint)commonLength < (uint)first.Length; commonLength++) {
            if (commonLength >= second.Length ||
                char.ToUpperInvariant(first[commonLength]) != char.ToUpperInvariant(second[commonLength])) {
                break;
            }
        }

        return commonLength;
    }

    private static bool AreRootsEqual(ReadOnlySpan<char> first, ReadOnlySpan<char> second, StringComparison comparisonType) {
        int firstRootLength = GetRootLength(first);
        int secondRootLength = GetRootLength(second);

        return firstRootLength == secondRootLength &&
               first[..firstRootLength].Equals(second[..secondRootLength], comparisonType);
    }

}
