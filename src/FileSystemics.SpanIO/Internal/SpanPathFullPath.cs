namespace FileSystemics.IO.Internal;

/// <summary>
/// Platform-aware full path normalization without <see cref="System.IO.Path.GetFullPath(string)"/>.
/// </summary>
internal static class SpanPathFullPath {
    private const int ExtraCapacity = 16;

    internal static int EstimateBufferCapacity(ReadOnlySpan<char> path, ReadOnlySpan<char> currentDirectory) =>
        EstimateCombineCapacity(path, currentDirectory);

    internal static bool RequiresFullNormalization(ReadOnlySpan<char> path) => NeedsExpansion(path);

    internal static bool TryGetFullPath(
        ReadOnlySpan<char> path,
        Span<char> destination,
        ReadOnlySpan<char> currentDirectory,
        out int charsWritten) {
        charsWritten = 0;
        if (path.IsEmpty || path.Contains('\0') || PathArgumentValidation.IsEffectivelyEmpty(path)) {
            return false;
        }

        if (NativePlatformTable.UsesUtf16NativePaths && IsExtended(path)) {
            if (destination.Length < path.Length) {
                return false;
            }

            path.CopyTo(destination);
            charsWritten = path.Length;
            return true;
        }

        if (!NeedsExpansion(path)) {
            return TryNormalize(path, destination, out charsWritten);
        }

        int combineCapacity = EstimateCombineCapacity(path, currentDirectory);
        if (destination.Length >= combineCapacity) {
            if (!TryExpand(path, currentDirectory, destination, out int combinedLength)) {
                return false;
            }

            return TryNormalize(destination[..combinedLength], destination, out charsWritten);
        }

        char[] rented = GC.AllocateUninitializedArray<char>(combineCapacity);
        if (!TryExpand(path, currentDirectory, rented, out int heapCombinedLength)) {
            return false;
        }

        return TryNormalize(rented.AsSpan(0, heapCombinedLength), destination, out charsWritten);
    }

    private static int EstimateCombineCapacity(ReadOnlySpan<char> path, ReadOnlySpan<char> currentDirectory) {
        if (NeedsExpansion(path)) {
            if (NativePlatformTable.UsesUtf16NativePaths) {
                if (IsCurrentDriveRooted(path)) {
                    return GetRootLength(currentDirectory) + path.Length;
                }

                if (IsDriveRelative(path)) {
                    return currentDirectory.Length + path.Length + ExtraCapacity;
                }
            }

            return currentDirectory.Length + path.Length + 1;
        }

        return path.Length;
    }

    private static bool TryExpand(
        ReadOnlySpan<char> path,
        ReadOnlySpan<char> currentDirectory,
        Span<char> destination,
        out int charsWritten) {
        charsWritten = 0;
        if (!NeedsExpansion(path)) {
            if (destination.Length < path.Length) {
                return false;
            }

            path.CopyTo(destination);
            charsWritten = path.Length;
            return true;
        }

        if (!NativePlatformTable.UsesUtf16NativePaths) {
            return IO.SpanPath.TryJoin(currentDirectory, path, destination, out charsWritten);
        }

        if (IsCurrentDriveRooted(path)) {
            ReadOnlySpan<char> root = GetPathRoot(currentDirectory);
            if (!IO.SpanPath.TryJoin(root, path[1..], destination, out charsWritten)) {
                return false;
            }

            return true;
        }

        if (IsDriveRelative(path)) {
            return TryExpandDriveRelative(path, currentDirectory, destination, out charsWritten);
        }

        return IO.SpanPath.TryJoin(currentDirectory, path, destination, out charsWritten);
    }

    private static bool TryExpandDriveRelative(
        ReadOnlySpan<char> path,
        ReadOnlySpan<char> currentDirectory,
        Span<char> destination,
        out int charsWritten) {
        charsWritten = 0;
        ReadOnlySpan<char> pathVolume = GetVolumeName(path);
        ReadOnlySpan<char> baseVolume = GetVolumeName(currentDirectory);
        if (pathVolume.Equals(baseVolume, NativePlatformTable.PathComparison)) {
            return IO.SpanPath.TryJoin(currentDirectory, path[2..], destination, out charsWritten);
        }

        if (IsDevice(currentDirectory)) {
            if (path.Length == 2) {
                if (!TryJoinDeviceRoot(currentDirectory, path, destination, out charsWritten)) {
                    return false;
                }

                return true;
            }

            if (!TryJoinDeviceRoot(currentDirectory, path[..2], destination, out int prefixLength)) {
                return false;
            }

            return IO.SpanPath.TryJoin(destination[..prefixLength], path[2..], destination, out charsWritten);
        }

        if (path.Length == 2) {
            if (destination.Length < 3) {
                return false;
            }

            destination[0] = path[0];
            destination[1] = VolumeSeparatorChar;
            destination[2] = DirectorySeparatorChar;
            charsWritten = 3;
            return true;
        }

        if (destination.Length < path.Length + 1) {
            return false;
        }

        destination[0] = path[0];
        destination[1] = VolumeSeparatorChar;
        destination[2] = DirectorySeparatorChar;
        path[2..].CopyTo(destination[3..]);
        charsWritten = path.Length + 1;
        return true;
    }

    private static bool TryJoinDeviceRoot(
        ReadOnlySpan<char> deviceBase,
        ReadOnlySpan<char> drive,
        Span<char> destination,
        out int charsWritten) {
        charsWritten = 0;
        const int DevicePrefixLength = 4;
        if (deviceBase.Length < DevicePrefixLength) {
            return false;
        }

        int needed = DevicePrefixLength + drive.Length + 1;
        if (destination.Length < needed) {
            return false;
        }

        deviceBase[..DevicePrefixLength].CopyTo(destination);
        destination[DevicePrefixLength] = DirectorySeparatorChar;
        drive.CopyTo(destination[(DevicePrefixLength + 1)..]);
        charsWritten = needed;
        return true;
    }

    private static bool TryNormalize(ReadOnlySpan<char> path, Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        char altSeparator = AltDirectorySeparatorChar;
        char separator = DirectorySeparatorChar;
        bool normalize = false;
        if (altSeparator != separator) {
            for (int i = 0; i < path.Length; i++) {
                if (path[i] == altSeparator) {
                    normalize = true;
                    break;
                }
            }
        }

        if (!normalize) {
            for (int i = 0; i < path.Length; i++) {
                char c = path[i];
                if (c == '.') {
                    if (i + 1 >= path.Length) {
                        continue;
                    }

                    if (path[i + 1] == '.') {
                        if (i + 2 >= path.Length || IsDirectorySeparator(path[i + 2])) {
                            normalize = true;
                            break;
                        }
                    }
                    else if (IsDirectorySeparator(path[i + 1]) || (i > 0 && IsDirectorySeparator(path[i - 1]))) {
                        normalize = true;
                        break;
                    }
                }
                else if (i > 0 && IsDirectorySeparator(c) && IsDirectorySeparator(path[i - 1])) {
                    normalize = true;
                    break;
                }
            }
        }

        if (!normalize) {
            charsWritten = path.Length;
            if (destination.Length < path.Length) {
                return false;
            }

            if (!path.SequenceEqual(destination[..path.Length])) {
                path.CopyTo(destination);
            }

            return true;
        }

        if (NativePlatformTable.UsesUtf16NativePaths && IsDevice(path)) {
            return TryRemoveRelativeSegments(path, GetRootLength(path), destination, out charsWritten);
        }

        int rootLength = GetRootLength(path);
        if (!TryRemoveRelativeSegments(path, rootLength, destination, out charsWritten)) {
            return false;
        }

        if (charsWritten == 0) {
            if (destination.Length < 1) {
                return false;
            }

            destination[0] = DirectorySeparatorChar;
            charsWritten = 1;
        }

        return true;
    }

    private static bool TryRemoveRelativeSegments(
        ReadOnlySpan<char> path,
        int rootLength,
        Span<char> destination,
        out int charsWritten) {
        charsWritten = 0;
        if (rootLength <= 0) {
            rootLength = 0;
        }

        int skip = rootLength;
        if (rootLength > 0 && IsDirectorySeparator(path[skip - 1])) {
            skip--;
        }

        SpanBuilder builder = new(destination);
        if (skip > 0 && !builder.TryAppend(path[..skip])) {
            return false;
        }

        for (int i = skip; i < path.Length; i++) {
            char c = path[i];
            if (IsDirectorySeparator(c) && i + 1 < path.Length) {
                if (IsDirectorySeparator(path[i + 1])) {
                    continue;
                }

                if ((i + 2 == path.Length || IsDirectorySeparator(path[i + 2])) && path[i + 1] == '.') {
                    i++;
                    continue;
                }

                if (i + 2 < path.Length &&
                    (i + 3 == path.Length || IsDirectorySeparator(path[i + 3])) &&
                    path[i + 1] == '.' && path[i + 2] == '.') {
                    int segmentEnd = builder.Length - 1;
                    while (segmentEnd >= skip) {
                        if (IsDirectorySeparator(destination[segmentEnd])) {
                            builder.Length = (i + 3 >= path.Length && segmentEnd == skip) ? segmentEnd + 1 : segmentEnd;
                            break;
                        }

                        segmentEnd--;
                    }

                    if (segmentEnd < skip) {
                        builder.Length = skip;
                    }

                    i += 2;
                    continue;
                }
            }

            if (c != DirectorySeparatorChar && c == AltDirectorySeparatorChar) {
                c = DirectorySeparatorChar;
            }

            if (!builder.TryAppend(c)) {
                return false;
            }
        }

        if (skip != rootLength && builder.Length < rootLength && rootLength > 0) {
            if (!builder.TryAppend(path[rootLength - 1])) {
                return false;
            }
        }

        charsWritten = builder.Length;
        return true;
    }

    private static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path) {
        int rootLength = GetRootLength(path);
        return rootLength == 0 ? ReadOnlySpan<char>.Empty : path[..rootLength];
    }

    private static ReadOnlySpan<char> GetVolumeName(ReadOnlySpan<char> path) {
        ReadOnlySpan<char> root = GetPathRoot(path);
        if (root.IsEmpty) {
            return root;
        }

        if (IsDevice(path)) {
            return root.Length > 4 ? root[4..] : ReadOnlySpan<char>.Empty;
        }

        return EndsInDirectorySeparator(root) ? root[..^1] : root;
    }

    private static bool NeedsExpansion(ReadOnlySpan<char> path) {
        if (NativePlatformTable.InvokeIsPartiallyQualified(path)) {
            return true;
        }

        return NativePlatformTable.UsesUtf16NativePaths &&
            (IsCurrentDriveRooted(path) || IsDriveRelative(path));
    }

    private static bool IsCurrentDriveRooted(ReadOnlySpan<char> path) =>
        path.Length >= 1 &&
        IsDirectorySeparator(path[0]) &&
        !(path.Length >= 2 && (path[1] == '?' || IsDirectorySeparator(path[1])));

    private static bool IsDriveRelative(ReadOnlySpan<char> path) =>
        path.Length >= 2 &&
        IsValidDriveChar(path[0]) &&
        path[1] == VolumeSeparatorChar &&
        (path.Length == 2 || !IsDirectorySeparator(path[2]));

    private static bool IsExtended(ReadOnlySpan<char> path) =>
        path.Length >= 4 &&
        path[0] == DirectorySeparatorChar &&
        (path[1] == DirectorySeparatorChar || path[1] == '?') &&
        path[2] == '?' &&
        path[3] == DirectorySeparatorChar;

    private static bool IsDevice(ReadOnlySpan<char> path) =>
        path.Length >= 4 &&
        IsDirectorySeparator(path[0]) &&
        IsDirectorySeparator(path[1]) &&
        (path[2] == '.' || path[2] == '?') &&
        IsDirectorySeparator(path[3]);

    private static bool EndsInDirectorySeparator(ReadOnlySpan<char> path) =>
        path.Length > 0 && IsDirectorySeparator(path[^1]);

    private static int GetRootLength(ReadOnlySpan<char> path) {
        if (!NativePlatformTable.UsesUtf16NativePaths) {
            return !path.IsEmpty && path[0] == DirectorySeparatorChar ? 1 : 0;
        }

        return NativePlatformTable.InvokeGetRootLength(path);
    }

    private static bool IsValidDriveChar(char value) =>
        (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');

    private static bool IsDirectorySeparator(char c) =>
        NativePlatformTable.InvokeIsDirectorySeparator(c);

    private static char DirectorySeparatorChar => NativePlatformTable.DirectorySeparatorChar;

    private static char AltDirectorySeparatorChar => NativePlatformTable.AltDirectorySeparatorChar;

    private static char VolumeSeparatorChar => NativePlatformTable.VolumeSeparatorChar;
}
