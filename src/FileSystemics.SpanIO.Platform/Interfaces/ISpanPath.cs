namespace FileSystemics.Abstractions;

/// <summary>
/// Span-based path manipulation mirroring <c>System.IO.Path</c> operations.
/// </summary>
public interface ISpanPath {
    /// <summary>Character separating directory levels.</summary>
    char DirectorySeparatorChar { get; }

    /// <summary>Alternate character separating directory levels.</summary>
    char AltDirectorySeparatorChar { get; }

    /// <summary>Character separating volume from directory.</summary>
    char VolumeSeparatorChar { get; }

    /// <summary>Character separating paths in environment variables.</summary>
    char PathSeparator { get; }

    /// <summary>Returns the file name and extension of a path.</summary>
    ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path);

    /// <summary>Returns the directory information for a path.</summary>
    ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path);

    /// <summary>Returns the extension of a path.</summary>
    ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path);

    /// <summary>Returns the file name without extension.</summary>
    ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path);

    /// <summary>Returns the root portion of a path.</summary>
    ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path);

    /// <summary>Determines whether a path includes a file extension.</summary>
    bool HasExtension(ReadOnlySpan<char> path);

    /// <summary>Determines whether a path includes a root.</summary>
    bool IsPathRooted(ReadOnlySpan<char> path);

    /// <summary>Determines whether a path is fully qualified.</summary>
    bool IsPathFullyQualified(ReadOnlySpan<char> path);

    /// <summary>Determines whether a path ends with a directory separator.</summary>
    bool EndsInDirectorySeparator(ReadOnlySpan<char> path);

    /// <summary>Removes trailing directory separators when safe.</summary>
    ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path);

    /// <summary>Joins two paths into a destination buffer.</summary>
    bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten);

    /// <summary>Joins two paths, throwing when the destination is too small.</summary>
    ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination);

    /// <summary>Joins three paths into a destination buffer.</summary>
    bool TryJoin(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        ReadOnlySpan<char> path3,
        Span<char> destination,
        out int charsWritten);

    /// <summary>Joins three paths, throwing when the destination is too small.</summary>
    ReadOnlySpan<char> Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination);

    /// <summary>Combines two paths into a destination buffer.</summary>
    bool TryCombine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten);

    /// <summary>Combines two paths, throwing when the destination is too small.</summary>
    ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination);

    /// <summary>Combines three paths into a destination buffer.</summary>
    bool TryCombine(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        ReadOnlySpan<char> path3,
        Span<char> destination,
        out int charsWritten);

    /// <summary>Combines three paths, throwing when the destination is too small.</summary>
    ReadOnlySpan<char> Combine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination);

    /// <summary>Changes the extension of a path in a destination buffer.</summary>
    bool TryChangeExtension(
        ReadOnlySpan<char> path,
        ReadOnlySpan<char> extension,
        Span<char> destination,
        out int charsWritten);

    /// <summary>Changes the extension of a path, throwing when the destination is too small.</summary>
    ReadOnlySpan<char> ChangeExtension(ReadOnlySpan<char> path, ReadOnlySpan<char> extension, Span<char> destination);

    /// <summary>Gets a relative path into a destination buffer.</summary>
    bool TryGetRelativePath(
        ReadOnlySpan<char> relativeTo,
        ReadOnlySpan<char> path,
        Span<char> destination,
        out int charsWritten);

    /// <summary>Gets a relative path, throwing when the destination is too small.</summary>
    ReadOnlySpan<char> GetRelativePath(ReadOnlySpan<char> relativeTo, ReadOnlySpan<char> path, Span<char> destination);
}
