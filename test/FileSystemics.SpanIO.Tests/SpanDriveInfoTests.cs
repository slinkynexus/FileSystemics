using FileSystemics.IO;

namespace FileSystemics.SpanIO.Tests;

public class SpanDriveInfoTests {
    [Test]
    public async Task RootDrive_MatchesBcl() {
        string driveName = OperatingSystem.IsWindows() ? "C:\\" : "/";
        DriveInfo expected = new(driveName);
        SpanDriveInfo actual = new(driveName.AsSpan());

        string name = actual.Name.ToString();
        bool isReady = actual.IsReady;
        DriveType driveType = actual.DriveType;
        string driveFormat = actual.DriveFormat.ToString();
        string volumeLabel = actual.VolumeLabel.ToString();
        long availableFreeSpace = actual.AvailableFreeSpace;
        long totalFreeSpace = actual.TotalFreeSpace;
        long totalSize = actual.TotalSize;

        await Assert.That(name).IsEqualTo(expected.Name);
        await Assert.That(isReady).IsEqualTo(expected.IsReady);
        await Assert.That(driveType).IsEqualTo(expected.DriveType);
        await Assert.That(driveFormat).IsEqualTo(expected.DriveFormat);
        await Assert.That(volumeLabel).IsEqualTo(expected.VolumeLabel ?? string.Empty);

        DriveInfo refreshed = new(driveName);
        const long spaceToleranceBytes = 64L * 1024 * 1024;
        await Assert.That(Math.Abs(availableFreeSpace - refreshed.AvailableFreeSpace)).IsLessThanOrEqualTo(spaceToleranceBytes);
        await Assert.That(Math.Abs(totalFreeSpace - refreshed.TotalFreeSpace)).IsLessThanOrEqualTo(spaceToleranceBytes);
        await Assert.That(totalSize).IsEqualTo(refreshed.TotalSize);
    }

    [Test]
    public async Task TryGetDrives_IncludesRoot() {
        char[] namesBuffer = new char[8192];
        int[] nameLengths = new int[64];
        bool success = SpanDriveInfo.TryGetDrives(namesBuffer, nameLengths, out int driveCount);
        string[] bclDrives = DriveInfo.GetDrives().Select(static drive => drive.Name).ToArray();
        List<string> spanDrives = [];
        int offset = 0;
        for (int i = 0; i < driveCount; i++) {
            int length = nameLengths[i];
            spanDrives.Add(namesBuffer.AsSpan(offset, length).ToString());
            offset += length;
        }

        await Assert.That(success).IsTrue();
        await Assert.That(driveCount).IsGreaterThan(0);
        await Assert.That(spanDrives).IsEquivalentTo(bclDrives);
    }

    [Test]
    public async Task EnumerateDrives_Foreach() {
        char[] namesBuffer = new char[8192];
        List<string> drives = [];
        foreach (ReadOnlySpan<char> drive in SpanDriveInfo.EnumerateDrives(namesBuffer)) {
            drives.Add(drive.ToString());
        }

        string[] bclDrives = DriveInfo.GetDrives().Select(static drive => drive.Name).ToArray();
        await Assert.That(drives).IsEquivalentTo(bclDrives);
    }

    [Test]
    public async Task Constructor_EmptyDriveName_ThrowsArgumentException() {
        SpanIOException? exception = null;
        try {
            SpanDriveInfo info = new(ReadOnlySpan<char>.Empty);
            _ = info.Name;
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("driveName");
    }

    [Test]
    public async Task Constructor_NullCharacter_ThrowsArgumentException() {
        SpanIOException? exception = null;
        try {
            SpanDriveInfo info = new("bad\0drive".AsSpan());
            _ = info.Name;
        }
        catch (SpanIOException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.ParamName).IsEqualTo("driveName");
    }

    [Test]
    public async Task SetVolumeLabel_OnUnix_ThrowsPlatformNotSupportedException() {
        if (OperatingSystem.IsWindows()) {
            return;
        }

        SpanDriveInfo drive = new("/".AsSpan());
        PlatformNotSupportedException? exception = null;
        try {
            drive.SetVolumeLabel("label".AsSpan());
        }
        catch (PlatformNotSupportedException ex) {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }
}
