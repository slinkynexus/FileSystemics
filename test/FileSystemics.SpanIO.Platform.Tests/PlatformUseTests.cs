namespace FileSystemics.SpanIO.Platform.Tests;

using FileSystemics.IO;

[NotInParallel("platform-binding")]
public partial class PlatformUseTests {
    [Test]
    public async Task Use_RebindsPathSeparatorsAndRestoresOnDispose() {
        char nativeSeparator = SpanPath.DirectorySeparatorChar;
        IPlatformHost host = new BackslashPathHost(Platform.Actual);

        using (Platform.Use(host)) {
            await Assert.That(SpanPath.DirectorySeparatorChar).IsEqualTo('\\');
            await Assert.That(SpanPath.AltDirectorySeparatorChar).IsEqualTo('/');
        }

        await Assert.That(SpanPath.DirectorySeparatorChar).IsEqualTo(nativeSeparator);
    }

    [Test]
    public async Task NestedUse_RestoresOuterHostAfterInnerDispose() {
        IPlatformHost outer = new BackslashPathHost(Platform.Actual);
        IPlatformHost inner = new ForwardSlashPathHost(Platform.Actual);

        using (Platform.Use(outer)) {
            await Assert.That(SpanPath.DirectorySeparatorChar).IsEqualTo('\\');

            using (Platform.Use(inner)) {
                await Assert.That(SpanPath.DirectorySeparatorChar).IsEqualTo('/');
            }

            await Assert.That(SpanPath.DirectorySeparatorChar).IsEqualTo('\\');
        }

        await Assert.That(SpanPath.DirectorySeparatorChar).IsEqualTo(OperatingSystem.IsWindows() ? '\\' : '/');
    }

    [Test]
    public async Task GetDirectoryName_UsesReboundSeparators() {
        IPlatformHost host = new BackslashPathHost(Platform.Actual);

        using (Platform.Use(host)) {
            await Assert.That(SpanPath.GetDirectoryName(@"dir\subdir\file.txt".AsSpan()).ToString()).IsEqualTo(@"dir\subdir");
            await Assert.That(SpanPath.GetDirectoryName(@"dir\subdir\".AsSpan()).ToString()).IsEqualTo(@"dir\subdir");
            await Assert.That(SpanPath.GetDirectoryName(@"file.txt".AsSpan()).ToString()).IsEqualTo(string.Empty);
            await Assert.That(SpanPath.GetExtension(@"dir\subdir\archive.tar.gz".AsSpan()).ToString()).IsEqualTo(".gz");
        }
    }


}
