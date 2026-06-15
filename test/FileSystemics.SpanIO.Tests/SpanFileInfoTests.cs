using FileSystemics.IO;

namespace FileSystemics.SpanIO.Tests;

public class SpanFileInfoTests {
    [Test]
    public async Task Exists() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("present.txt", "x".AsSpan());
        bool exists = new SpanFileInfo(filePath.AsSpan()).Exists;

        await Assert.That(exists).IsTrue();
        await Assert.That(new FileInfo(filePath).Exists).IsTrue();
    }

    [Test]
    public async Task NameAndExtension() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("nested/report.pdf", "x".AsSpan());
        SpanFileInfo info = new(filePath.AsSpan());
        string name = info.Name.ToString();
        string extension = info.Extension.ToString();
        string fullName = info.FullName.ToString();

        await Assert.That(name).IsEqualTo("report.pdf");
        await Assert.That(extension).IsEqualTo(".pdf");
        await Assert.That(fullName).IsEqualTo(filePath);
    }

    [Test]
    public async Task Length() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("bytes.bin", "abcd".AsSpan());
        long length = new SpanFileInfo(filePath.AsSpan()).Length;

        await Assert.That(length).IsEqualTo(4);
    }

    [Test]
    public async Task Delete() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("delete-me.txt", "x".AsSpan());
        SpanFileInfo info = new(filePath.AsSpan());
        info.Delete();
        bool exists = info.Exists;

        await Assert.That(exists).IsFalse();
    }

    [Test]
    public async Task CopyTo() {
        using TempDirectoryScope temp = new();
        string sourcePath = temp.CreateFile("source.txt", "payload".AsSpan());
        string destinationPath = temp.CreateEmptyFile("destination.txt");
        SpanFileInfo source = new(sourcePath.AsSpan());
        SpanFileInfo destination = new(destinationPath.AsSpan());
        source.CopyTo(destination, overwrite: true);

        await Assert.That(File.ReadAllText(destinationPath)).IsEqualTo("payload");
    }

    [Test]
    public async Task MoveTo() {
        using TempDirectoryScope temp = new();
        string sourcePath = temp.CreateFile("source.txt", "payload".AsSpan());
        string destinationPath = temp.Combine("moved.txt");
        new SpanFileInfo(sourcePath.AsSpan()).MoveTo(destinationPath.AsSpan());

        await Assert.That(File.Exists(destinationPath)).IsTrue();
        await Assert.That(File.Exists(sourcePath)).IsFalse();
    }

    [Test]
    public async Task OpenRead() {
        using TempDirectoryScope temp = new();
        string filePath = temp.CreateFile("open.txt", "hello".AsSpan());
        using FileStream stream = new SpanFileInfo(filePath.AsSpan()).OpenRead();

        await Assert.That(stream.CanRead).IsTrue();
    }

    [Test]
    public async Task TryReplace() {
        using TempDirectoryScope temp = new();
        string targetPath = temp.CreateFile("target.txt", "old".AsSpan());
        string replacementPath = temp.CreateFile("replacement.txt", "new".AsSpan());
        string backupPath = temp.CreateEmptyFile("backup.txt");
        bool replaced = new SpanFileInfo(targetPath.AsSpan()).TryReplace(replacementPath.AsSpan(), backupPath.AsSpan());

        await Assert.That(replaced).IsTrue();
        await Assert.That(File.ReadAllText(targetPath)).IsEqualTo("new");
        await Assert.That(File.ReadAllText(backupPath)).IsEqualTo("old");
    }

    [Test]
    public async Task Constructor_NullCharacter_ThrowsArgumentException() {
        SpanIOException? exception = null;
        try {
            SpanFileInfo info = new("bad\0path".AsSpan());
            _ = info.Exists;
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("path");
    }
}
