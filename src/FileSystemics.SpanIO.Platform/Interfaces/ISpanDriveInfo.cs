namespace FileSystemics.Abstractions;

/// <summary>
/// Drive metadata mirroring <see cref="DriveInfo"/>.
/// </summary>
public interface ISpanDriveInfo {
    /// <summary>Gets the drive name.</summary>
    ReadOnlySpan<char> Name { get; }

    /// <summary>Gets whether the drive is ready.</summary>
    bool IsReady { get; }

    /// <summary>Gets the root directory of the drive.</summary>
    ISpanDirectoryInfo RootDirectory { get; }

    /// <summary>Gets the drive type.</summary>
    DriveType DriveType { get; }

    /// <summary>Gets the file-system format of the drive.</summary>
    ReadOnlySpan<char> DriveFormat { get; }

    /// <summary>Gets the volume label.</summary>
    ReadOnlySpan<char> VolumeLabel { get; }

    /// <summary>Gets available free space in bytes.</summary>
    long AvailableFreeSpace { get; }

    /// <summary>Gets total free space in bytes.</summary>
    long TotalFreeSpace { get; }

    /// <summary>Gets total drive size in bytes.</summary>
    long TotalSize { get; }

    /// <summary>Sets the volume label.</summary>
    void SetVolumeLabel(ReadOnlySpan<char> label);
}
