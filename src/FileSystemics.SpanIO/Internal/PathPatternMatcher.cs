namespace FileSystemics.IO.Internal;

internal static class PathPatternMatcher {
    internal static bool Matches(ReadOnlySpan<char> name, ReadOnlySpan<char> pattern) {
        if (pattern.IsEmpty || pattern.SequenceEqual("*")) {
            return true;
        }

        if (pattern[0] == '*') {
            return name.EndsWith(pattern[1..]);
        }

        if (pattern[^1] == '*') {
            return name.StartsWith(pattern[..^1]);
        }

        return name.SequenceEqual(pattern);
    }
}
