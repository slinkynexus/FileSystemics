namespace FileSystemics.IO;

/// <summary>
/// Thrown when span-based path APIs receive invalid arguments or insufficient destination buffers.
/// </summary>
public sealed class SpanIOException(string message, string? paramName) : ArgumentException(message, paramName) {
    internal static SpanIOException EmptyPath(string paramName) =>
        new("The value cannot be an empty string.", paramName);

    internal static SpanIOException DestinationTooSmall() =>
        new("The destination buffer is too small.", "destination");

    internal static SpanIOException NullCharacterInPath(string paramName) =>
        new("Null character in path.", paramName);

    internal static SpanIOException RootedSubPath(string paramName) =>
        new("Second path fragment must not be rooted.", paramName);

    internal static SpanIOException InvalidSubPath(ReadOnlySpan<char> subPath, ReadOnlySpan<char> parentPath, string paramName) =>
        new($"The directory specified, \"{subPath}\", is not a subdirectory of \"{parentPath}\".", paramName);

    internal static SpanIOException MustBeNonEmptyDriveName(string paramName) =>
        new("The drive name must be non-empty.", paramName);

    internal static SpanIOException MustBeDriveLetterOrRootDirectory(string paramName) =>
        new("The drive name must be a valid drive letter or rooted directory.", paramName);

    internal static SpanIOException InvalidDriveCharacters(ReadOnlySpan<char> driveName, string paramName) =>
        new($"The drive name '{driveName}' contains invalid characters.", paramName);

    internal static SpanIOException InvalidDriveCharacters(string paramName) =>
        new("The drive name contains invalid characters.", paramName);
}
