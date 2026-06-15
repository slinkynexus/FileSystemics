namespace FileSystemics.IO;

internal sealed class OwnedSpanPath {
    private char[] _chars;

    internal OwnedSpanPath(ReadOnlySpan<char> path) {
        _chars = new char[path.Length];
        path.CopyTo(_chars);
    }

    internal OwnedSpanPath(string path) {
        _chars = path.ToCharArray();
    }

    internal ReadOnlySpan<char> Span => _chars;

    internal void Replace(ReadOnlySpan<char> path) {
        _chars = new char[path.Length];
        path.CopyTo(_chars);
    }
}
