namespace FileSystemics.IO.Internal;

/// <summary>
/// Mirrors <c>FileSystem.VerifyValidPath</c> and related BCL path argument checks.
/// </summary>
internal static class PathArgumentValidation {
    internal static void ValidatePath(ReadOnlySpan<char> path, string paramName) {
        if (path.IsEmpty) {
            throw SpanIOException.EmptyPath(paramName);
        }

        if (path.Contains('\0')) {
            throw SpanIOException.NullCharacterInPath(paramName);
        }
    }

    internal static bool ShouldExistenceCheckReturnFalse(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeShouldExistenceCheckReturnFalse(path);

    internal static bool IsEffectivelyEmpty(ReadOnlySpan<char> path) =>
        NativePlatformTable.InvokeIsEffectivelyEmpty(path);
}
