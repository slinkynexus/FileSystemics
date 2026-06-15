using FileSystemics.Abstractions;

namespace FileSystemics.IO;

internal sealed class SpanDriveInfoFactoryAdapter(ISpanFileSystem fileSystem) : ISpanDriveInfoFactory {
    public ISpanFileSystem FileSystem { get; } = fileSystem;

    public ISpanDriveInfo[] GetDrives() {
        List<ISpanDriveInfo> drives = [];
        char[] nameBuffer = new char[1024];
        SpanDriveEnumerator enumerator = SpanDriveInfo.EnumerateDrives(nameBuffer);
        try {
            while (enumerator.MoveNext()) {
                drives.Add(new SpanDriveInfoAdapter(FileSystem, enumerator.Current));
            }
        }
        finally {
            enumerator.Dispose();
        }

        return drives.ToArray();
    }

    public ISpanDriveInfo New(ReadOnlySpan<char> driveName) => new SpanDriveInfoAdapter(FileSystem, driveName);

    public ISpanDriveInfo? Wrap(DriveInfo? driveInfo) =>
        driveInfo is null ? null : new SpanDriveInfoAdapter(FileSystem, driveInfo.Name);
}
