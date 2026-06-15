using FileSystemics.IO;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace FileSystemics.SpanIO.Tests;

public class SpanFileTests {
    [Test]
    public async Task Exists() {
        await Assert.That(SpanFile.Exists(ReadOnlySpan<char>.Empty)).IsFalse();

        using TempDirectoryScope temp = new();
        string missing = temp.Combine("does-not-exist.bin");
        await Assert.That(SpanFile.Exists(missing.AsSpan())).IsFalse();
        await Assert.That(SpanFile.Exists(temp.Path.AsSpan())).IsFalse();

        string filePath = temp.CreateFile("present.txt", "x".AsSpan());
        await Assert.That(SpanFile.Exists(filePath.AsSpan())).IsTrue();
        await Assert.That(File.Exists(filePath)).IsTrue();
    }

    [Test]
    public async Task OpenHandle() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("open.txt", "payload".AsSpan());

        using SafeFileHandle handle = SpanFile.OpenHandle(filePath.AsSpan());
        await Assert.That(handle.IsInvalid).IsFalse();

        SpanIOException? emptyPath = null;
        try {
            SpanFile.OpenHandle(ReadOnlySpan<char>.Empty);
        }
        catch (SpanIOException ex) {
            emptyPath = ex;
        }

        await Assert.That(emptyPath).IsNotNull();
    }

    [Test]
    public async Task Open() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("open-stream.txt", "stream payload".AsSpan());

        using FileStream stream = SpanFile.Open(filePath.AsSpan(), FileMode.Open, FileAccess.Read, FileShare.Read);
        await Assert.That(stream.CanRead).IsTrue();
        await Assert.That(stream.CanWrite).IsFalse();

        using FileStream readStream = SpanFile.OpenRead(filePath.AsSpan());
        await Assert.That(readStream.CanRead).IsTrue();

        string writePath = temp.Combine("write-stream.txt");
        using (FileStream writeStream = SpanFile.OpenWrite(writePath.AsSpan())) {
            writeStream.Write("written"u8);
        }

        await Assert.That(await File.ReadAllTextAsync(writePath)).IsEqualTo("written");

        string createPath = temp.Combine("create-stream.txt");
        using (FileStream createStream = SpanFile.Create(createPath.AsSpan())) {
            createStream.Write("created"u8);
        }

        await Assert.That(await File.ReadAllTextAsync(createPath)).IsEqualTo("created");

        using StreamReader reader = SpanFile.OpenText(filePath.AsSpan());
        await Assert.That(await reader.ReadToEndAsync()).IsEqualTo("stream payload");

        string textPath = temp.Combine("text-stream.txt");
        using (StreamWriter writer = SpanFile.CreateText(textPath.AsSpan())) {
            await writer.WriteAsync("create text");
        }

        await Assert.That(await File.ReadAllTextAsync(textPath)).IsEqualTo("create text");

        using (StreamWriter appender = SpanFile.AppendText(textPath.AsSpan())) {
            await appender.WriteAsync(" appended");
        }

        await Assert.That(await File.ReadAllTextAsync(textPath)).IsEqualTo("create text appended");
    }

    [Test]
    public async Task Open_DirectoryAndFileName() {
        using TempDirectoryScope temp = new();
        string fileName = "nested-open.txt";
        temp.CreateFile(fileName, "combined open stream".AsSpan());

        using FileStream stream = SpanFile.Open(temp.Path.AsSpan(), fileName.AsSpan(), FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new(stream);
        await Assert.That(await reader.ReadToEndAsync()).IsEqualTo("combined open stream");
    }

    [Test]
    public async Task Replace() {
        using TempDirectoryScope temp = new();
        string source = temp.CreateFile("source.txt", "replacement".AsSpan());
        string destination = temp.CreateFile("destination.txt", "original".AsSpan());
        string backup = temp.Combine("backup.txt");

        SpanFile.Replace(source.AsSpan(), destination.AsSpan(), backup.AsSpan());
        await Assert.That(await File.ReadAllTextAsync(destination)).IsEqualTo("replacement");
        await Assert.That(await File.ReadAllTextAsync(backup)).IsEqualTo("original");
    }

    [Test]
    public async Task OpenHandle_DirectoryAndFileName() {
        using TempDirectoryScope temp = new();
        string fileName = "nested.txt";
        temp.CreateFile(fileName, "combined open".AsSpan());

        using SafeFileHandle handle = SpanFile.OpenHandle(temp.Path.AsSpan(), fileName.AsSpan());
        await Assert.That(handle.IsInvalid).IsFalse();

        SpanIOException? emptyFileName = null;
        try {
            SpanFile.OpenHandle(temp.Path.AsSpan(), ReadOnlySpan<char>.Empty);
        }
        catch (SpanIOException ex) {
            emptyFileName = ex;
        }

        await Assert.That(emptyFileName).IsNotNull();
    }

    [Test]
    public async Task Delete() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("delete-me.txt", "x".AsSpan());

        SpanFile.Delete(filePath.AsSpan());
        await Assert.That(File.Exists(filePath)).IsFalse();

        Exception? missing = null;
        try {
            SpanFile.Delete(temp.Combine("missing.txt").AsSpan());
        }
        catch (Exception ex) {
            missing = ex;
        }

        await Assert.That(missing).IsNotNull();

        SpanIOException? emptyPath = null;
        try {
            SpanFile.Delete(ReadOnlySpan<char>.Empty);
        }
        catch (SpanIOException ex) {
            emptyPath = ex;
        }

        await Assert.That(emptyPath).IsNotNull();
    }

    [Test]
    public async Task Copy() {
        using TempDirectoryScope temp = new();
        string source = temp.CreateFile("source.txt", "new".AsSpan());
        string destination = temp.CreateFile("destination.txt", "old".AsSpan());

        SpanFile.Copy(source.AsSpan(), destination.AsSpan(), overwrite: true);
        await Assert.That(await File.ReadAllTextAsync(destination)).IsEqualTo("new");

        IOException? noOverwrite = null;
        try {
            SpanFile.Copy(source.AsSpan(), destination.AsSpan(), overwrite: false);
        }
        catch (IOException ex) {
            noOverwrite = ex;
        }

        await Assert.That(noOverwrite).IsNotNull();

        Exception? missingSource = null;
        try {
            SpanFile.Copy(temp.Combine("missing.txt").AsSpan(), destination.AsSpan());
        }
        catch (Exception ex) {
            missingSource = ex;
        }

        await Assert.That(missingSource).IsNotNull();
    }

    [Test]
    public async Task Move() {
        using TempDirectoryScope temp = new();
        string source = temp.CreateFile("source.txt", "payload".AsSpan());
        string destination = temp.Combine("moved.txt");

        SpanFile.Move(source.AsSpan(), destination.AsSpan());

        await Assert.That(File.Exists(source)).IsFalse();
        await Assert.That(File.Exists(destination)).IsTrue();
    }

    [Test]
    public async Task ReadAllBytes() {
        using TempDirectoryScope temp = new();
        byte[] payload = [1, 2, 3, 4, 5];
        string filePath = temp.CreateEmptyFile("bytes.bin");
        await File.WriteAllBytesAsync(filePath, payload);

        byte[] read = SpanFile.ReadAllBytes(filePath.AsSpan());
        await Assert.That(read).IsEquivalentTo(payload);
    }

    [Test]
    public async Task WriteAllBytes() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateEmptyFile("write-bytes.bin");
        byte[] payload = [9, 8, 7];

        SpanFile.WriteAllBytes(filePath.AsSpan(), payload);
        await Assert.That(await File.ReadAllBytesAsync(filePath)).IsEquivalentTo(payload);
    }

    [Test]
    public async Task ReadAllText() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("read.txt", "hello span io".AsSpan());

        string defaultEncoding = SpanFile.ReadAllText(filePath.AsSpan());
        string utf8 = SpanFile.ReadAllText(filePath.AsSpan(), Encoding.UTF8);

        await Assert.That(defaultEncoding).IsEqualTo("hello span io");
        await Assert.That(utf8).IsEqualTo("hello span io");
    }

    [Test]
    public async Task WriteAllText() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateEmptyFile("write.txt");

        SpanFile.WriteAllText(filePath.AsSpan(), "hello span io".AsSpan());
        await Assert.That(await File.ReadAllTextAsync(filePath)).IsEqualTo("hello span io");

        SpanFile.WriteAllText(filePath.AsSpan(), ReadOnlySpan<char>.Empty);
        await Assert.That((await File.ReadAllBytesAsync(filePath)).Length).IsEqualTo(0);

        SpanFile.WriteAllText(filePath.AsSpan(), "unicode".AsSpan(), Encoding.UTF8);
        await Assert.That(await File.ReadAllTextAsync(filePath, Encoding.UTF8)).IsEqualTo("unicode");
    }

    [Test]
    public async Task GetAttributes() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("attrs.txt", "x".AsSpan());

        FileAttributes attributes = SpanFile.GetAttributes(filePath.AsSpan());
        await Assert.That(attributes.HasFlag(FileAttributes.Directory)).IsFalse();
    }

    [Test]
    public async Task SetAttributes() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("readonly.txt", "initial".AsSpan());

        SpanFile.SetAttributes(filePath.AsSpan(), FileAttributes.ReadOnly);
        FileAttributes attributes = File.GetAttributes(filePath);
        await Assert.That(attributes.HasFlag(FileAttributes.ReadOnly)).IsTrue();

        SpanFile.SetAttributes(filePath.AsSpan(), FileAttributes.Normal);
    }

    [Test]
    public async Task SetLastWriteTimeUtc() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("mtime.txt", "x".AsSpan());
        DateTime expected = new(2024, 3, 10, 16, 45, 0, DateTimeKind.Utc);

        SpanFile.SetLastWriteTimeUtc(filePath.AsSpan(), expected);

        await Assert.That(SpanFile.GetLastWriteTimeUtc(filePath.AsSpan())).IsEqualTo(expected);
        await Assert.That(File.GetLastWriteTimeUtc(filePath)).IsEqualTo(expected);
    }

    [Test]
    public async Task SetUnixFileMode() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("mode.txt", "x".AsSpan());
        UnixFileMode expected = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead;

        SpanFile.SetUnixFileMode(filePath.AsSpan(), expected);

        await Assert.That(SpanFile.GetUnixFileMode(filePath.AsSpan())).IsEqualTo(expected);
        await Assert.That(File.GetUnixFileMode(filePath)).IsEqualTo(expected);
    }

    [Test]
    public async Task Exists_NullCharacter_ReturnsFalse() {
        bool exists = SpanFile.Exists("missing\0file".AsSpan());

        await Assert.That(exists).IsFalse();
        await Assert.That(File.Exists("missing\0file")).IsFalse();
    }

    [Test]
    public async Task Delete_NullCharacter_ThrowsArgumentException() {
        SpanIOException? exception = null;
        try {
            SpanFile.Delete("bad\0path".AsSpan());
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }
}
