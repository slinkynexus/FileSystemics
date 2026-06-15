using FileSystemics.IO;
using FileSystemics.IO.Internal;

namespace FileSystemics.SpanIO.Tests;

public class PlatformPathBufferTests {
    [Test]
    public async Task WithPath_RejectsEmbeddedNull() {
        SpanIOException? exception = null;
        try {
            PlatformPath.WithPath(
                "bad\0path".AsSpan(),
                (_, _) => 0,
                _ => 0);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task WithCombinedPath_RejectsEmbeddedNullInDirectory() {
        SpanIOException? exception = null;
        try {
            PlatformPath.WithCombinedPath(
                "bad\0dir".AsSpan(),
                "file.txt".AsSpan(),
                '/',
                (_, _) => 0,
                _ => 0);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task WithCombinedPath_RejectsEmbeddedNullInFileName() {
        SpanIOException? exception = null;
        try {
            PlatformPath.WithCombinedPath(
                "/tmp".AsSpan(),
                "bad\0name".AsSpan(),
                '/',
                (_, _) => 0,
                _ => 0);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task WithTwoPaths_RejectsEmbeddedNull() {
        SpanIOException? exception = null;
        try {
            PlatformPath.WithTwoPaths(
                "a\0".AsSpan(),
                "/b".AsSpan(),
                (_, _) => 0,
                (_, _) => 0);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task WithCombinedPath_EncodesDirectoryAndFileName() {
        int result = PlatformPath.WithCombinedPath(
            "/tmp".AsSpan(),
            "file.txt".AsSpan(),
            '/',
            (_, _) => 1,
            utf8 => {
                string decoded = System.Text.Encoding.UTF8.GetString(utf8[..^1]);
                return decoded == "/tmp/file.txt" ? 2 : 0;
            });

        if (OperatingSystem.IsWindows()) {
            await Assert.That(result).IsEqualTo(1);
        }
        else {
            await Assert.That(result).IsEqualTo(2);
        }
    }

    [Test]
    public async Task WithCombinedPath_InsertsSeparatorWhenMissing() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        int result = PlatformPath.WithCombinedPath(
            "/tmp".AsSpan(),
            "file.txt".AsSpan(),
            '/',
            (_, _) => 0,
            utf8 => {
                string decoded = System.Text.Encoding.UTF8.GetString(utf8[..^1]);
                return decoded == "/tmp/file.txt" ? 1 : 0;
            });

        await Assert.That(result).IsEqualTo(1);
    }

    [Test]
    public async Task WithCombinedPath_DoesNotDuplicateSeparator() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        int result = PlatformPath.WithCombinedPath(
            "/tmp/".AsSpan(),
            "file.txt".AsSpan(),
            '/',
            (_, _) => 0,
            utf8 => {
                string decoded = System.Text.Encoding.UTF8.GetString(utf8[..^1]);
                return decoded == "/tmp/file.txt" ? 1 : 0;
            });

        await Assert.That(result).IsEqualTo(1);
    }

    [Test]
    public async Task TryCreate_EncodesNullTerminatedUtf8_OnUnix() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        PlatformPathBufferRental buffer = default;
        bool created = PlatformPathBuffer.TryCreate("/alpha".AsSpan(), useUtf16: false, ref buffer, out int encodedLength);
        string decoded;
        try {
            ReadOnlySpan<byte> utf8 = buffer.AsUtf8();
            decoded = System.Text.Encoding.UTF8.GetString(utf8[..^1]);
        }
        finally {
            buffer.Dispose();
        }

        await Assert.That(created).IsTrue();
        await Assert.That(encodedLength).IsEqualTo(6);
        await Assert.That(decoded).IsEqualTo("/alpha");
    }

    [Test]
    public async Task WithPath_EmptyPath_ThrowsArgumentException() {
        SpanIOException? exception = null;
        try {
            PlatformPath.WithPath(
                ReadOnlySpan<char>.Empty,
                (_, _) => 7,
                _ => 8);
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }
}
