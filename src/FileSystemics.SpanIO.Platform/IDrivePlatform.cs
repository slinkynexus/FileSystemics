namespace FileSystemics.IO;

/// <summary>
/// Drive naming, enumeration, and space queries for a platform host.
/// </summary>
public interface IDrivePlatform {
    /// <summary>Describes how logical drives are enumerated.</summary>
    DriveEnumerationKind EnumerationKind { get; }

    /// <summary>Gets whether the platform uses drive letters.</summary>
    bool UsesDriveLetters { get; }

    /// <summary>Gets whether the volume label is the mount path.</summary>
    bool VolumeLabelIsMountPath { get; }

    /// <summary>Returns the buffer capacity needed to normalize a drive name.</summary>
    int GetNormalizedDriveCapacity(ReadOnlySpan<char> driveName, string paramName);

    /// <summary>Normalizes a drive name into a destination buffer.</summary>
    int NormalizeDriveName(ReadOnlySpan<char> driveName, Span<char> destination, string paramName);

    /// <summary>Gets the drive type.</summary>
    DriveType GetDriveType(ReadOnlySpan<char> name);

    /// <summary>Gets the file-system format into a destination buffer.</summary>
    void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten);

    /// <summary>Gets the volume label into a destination buffer.</summary>
    void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten);

    /// <summary>Sets the volume label.</summary>
    void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label);

    /// <summary>Gets available free space in bytes.</summary>
    long GetAvailableFreeSpace(ReadOnlySpan<char> name);

    /// <summary>Gets total free space in bytes.</summary>
    long GetTotalFreeSpace(ReadOnlySpan<char> name);

    /// <summary>Gets total drive size in bytes.</summary>
    long GetTotalSize(ReadOnlySpan<char> name);

    /// <summary>Enumerates logical drives.</summary>
    IDriveEnumerator EnumerateDrives();
}
