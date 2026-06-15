using FileSystemics.Abstractions;

namespace FileSystemics.IO;

internal sealed class SpanDriveInfoAdapter : ISpanDriveInfo {
    private readonly ISpanFileSystem _fileSystem;
    private readonly OwnedSpanPath _name;
    private OwnedSpanPath? _driveFormat;
    private OwnedSpanPath? _volumeLabel;

    internal SpanDriveInfoAdapter(ISpanFileSystem fileSystem, ReadOnlySpan<char> driveName) {
        _fileSystem = fileSystem;
        SpanDriveInfo driveInfo = SpanInfoSource.Drive(driveName);
        _name = new OwnedSpanPath(driveInfo.Name);
    }

    internal SpanDriveInfoAdapter(ISpanFileSystem fileSystem, string driveName) : this(fileSystem, driveName.AsSpan()) { }

    public ReadOnlySpan<char> Name => _name.Span;

    public bool IsReady => SpanInfoSource.Drive(_name.Span).IsReady;

    public ISpanDirectoryInfo RootDirectory =>
        _fileSystem.DirectoryInfo.New(SpanInfoSource.Drive(_name.Span).RootDirectory.FullName);

    public DriveType DriveType => SpanInfoSource.Drive(_name.Span).DriveType;

    public ReadOnlySpan<char> DriveFormat {
        get {
            if (_driveFormat is null) {
                _driveFormat = new OwnedSpanPath(SpanInfoSource.Drive(_name.Span).DriveFormat);
            }

            return _driveFormat.Span;
        }
    }

    public ReadOnlySpan<char> VolumeLabel {
        get {
            if (_volumeLabel is null) {
                _volumeLabel = new OwnedSpanPath(SpanInfoSource.Drive(_name.Span).VolumeLabel);
            }

            return _volumeLabel.Span;
        }
    }

    public long AvailableFreeSpace => SpanInfoSource.Drive(_name.Span).AvailableFreeSpace;

    public long TotalFreeSpace => SpanInfoSource.Drive(_name.Span).TotalFreeSpace;

    public long TotalSize => SpanInfoSource.Drive(_name.Span).TotalSize;

    public void SetVolumeLabel(ReadOnlySpan<char> label) {
        SpanInfoSource.Drive(_name.Span).SetVolumeLabel(label);
        _volumeLabel = null;
    }
}
