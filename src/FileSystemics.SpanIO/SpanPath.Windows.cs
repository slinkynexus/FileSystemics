using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

public static partial class SpanPath {
    [ThreadStatic]
    private static char[]? t_windowsSeparatorScratch;

    private static ReadOnlySpan<char> CanonicalizeWindowsPathSeparators(ReadOnlySpan<char> path) {
        if (!OperatingSystem.IsWindows() || path.IsEmpty) {
            return path;
        }

        bool needsCanonicalization = false;
        for (int i = 0; i < path.Length; i++) {
            if (path[i] == AltDirectorySeparatorChar) {
                needsCanonicalization = true;
                break;
            }
        }

        if (!needsCanonicalization) {
            return path;
        }

        char[] scratch = t_windowsSeparatorScratch ??= new char[PlatformPathBuffer.STACK_THRESHOLD_CHARS];
        if (path.Length > scratch.Length) {
            scratch = new char[path.Length];
        }

        for (int i = 0; i < path.Length; i++) {
            char ch = path[i];
            scratch[i] = ch == AltDirectorySeparatorChar ? DirectorySeparatorChar : ch;
        }

        return scratch.AsSpan(0, path.Length);
    }
}
