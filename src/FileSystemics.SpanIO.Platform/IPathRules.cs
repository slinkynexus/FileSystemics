namespace FileSystemics.IO;

/// <summary>
/// Path semantics and native encoding rules for a platform host.
/// </summary>
public interface IPathRules {
    /// <summary>Character separating directory levels.</summary>
    char DirectorySeparatorChar { get; }

    /// <summary>Alternate character separating directory levels.</summary>
    char AltDirectorySeparatorChar { get; }

    /// <summary>Character separating volume from directory.</summary>
    char VolumeSeparatorChar { get; }

    /// <summary>Character separating paths in environment variables.</summary>
    char PathSeparator { get; }

    /// <summary>String comparison used for path equality.</summary>
    StringComparison PathComparison { get; }

    /// <summary>Gets whether native paths are UTF-16 encoded.</summary>
    bool UsesUtf16NativePaths { get; }

    /// <summary>Gets whether directory enumeration uses getdents64.</summary>
    bool UsesGetdents64DirectoryEnumeration { get; }

    /// <summary>Returns the length of the path root.</summary>
    int GetRootLength(ReadOnlySpan<char> path);

    /// <summary>Determines whether a path includes a root.</summary>
    bool IsPathRooted(ReadOnlySpan<char> path);

    /// <summary>Determines whether a path is partially qualified.</summary>
    bool IsPartiallyQualified(ReadOnlySpan<char> path);

    /// <summary>Determines whether a character is a directory separator.</summary>
    bool IsDirectorySeparator(char value);

    /// <summary>Determines whether existence checks should return false for a path.</summary>
    bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path);

    /// <summary>Determines whether a path is effectively empty.</summary>
    bool IsEffectivelyEmpty(ReadOnlySpan<char> path);

    /// <summary>Invokes an action with a native-encoded path.</summary>
    T WithNativePath<T>(
        ReadOnlySpan<char> path,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action);

    /// <summary>Invokes an action with a combined native-encoded path.</summary>
    T WithCombinedNativePath<T>(
        ReadOnlySpan<char> directory,
        ReadOnlySpan<char> fileName,
        char separator,
        Func<ReadOnlySpan<char>, ReadOnlySpan<byte>, T> action);

    /// <summary>Invokes an action with two native-encoded paths.</summary>
    T WithTwoNativePaths<T>(
        ReadOnlySpan<char> path1,
        ReadOnlySpan<char> path2,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>, T> utf16Action,
        Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, T> utf8Action);
}
