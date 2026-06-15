using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

/// <summary>
/// Span-based drive information mirroring <see cref="DriveInfo"/>.
/// </summary>
public ref struct SpanDriveInfo {
    private readonly ReadOnlySpan<char> _name;
#pragma warning disable CS0414, CS0169 // Retains the normalized drive-name allocation backing <see cref="Name"/>.
    private readonly char[]? _ownedName;
#pragma warning restore CS0414, CS0169
    private DriveInfoData? _metadata;

    /// <summary>Initializes drive metadata for the specified drive name.</summary>
    public SpanDriveInfo(ReadOnlySpan<char> driveName) {
        int capacity = DriveNameNormalizer.GetNormalizedCapacity(driveName, nameof(driveName));
        char[] name = new char[capacity];
        int length = DriveNameNormalizer.Normalize(driveName, name, nameof(driveName));
        _ownedName = name;
        _name = name.AsSpan(0, length);
    }

    /// <summary>Initializes drive metadata using a caller-provided name buffer.</summary>
    /// <remarks>The normalized name is written into <paramref name="nameBuffer"/>; its memory must remain valid while using this struct.</remarks>
    public SpanDriveInfo(ReadOnlySpan<char> driveName, Span<char> nameBuffer) {
        int length = DriveNameNormalizer.Normalize(driveName, nameBuffer, nameof(driveName));
        _name = nameBuffer[..length];
    }

    /// <summary>Gets the drive name.</summary>
    public ReadOnlySpan<char> Name => _name;

    /// <summary>Gets whether the drive is ready.</summary>
    public bool IsReady => SpanDirectory.Exists(_name);

    /// <summary>Gets the root directory of the drive.</summary>
    public SpanDirectoryInfo RootDirectory => new(_name);

    /// <summary>Gets the drive type.</summary>
    public DriveType DriveType => DriveInfoQueries.GetDriveType(_name);

    /// <summary>Gets the file-system format of the drive.</summary>
    public ReadOnlySpan<char> DriveFormat {
        get {
            DriveInfoData metadata = Metadata;
            DriveInfoQueries.GetDriveFormat(_name, metadata.FormatBuffer, out int length);
            return metadata.FormatBuffer.AsSpan(0, length);
        }
    }

    /// <summary>Gets the volume label.</summary>
    public ReadOnlySpan<char> VolumeLabel {
        get {
            if (NativePlatformTable.VolumeLabelIsMountPath) {
                return _name;
            }

            DriveInfoData metadata = Metadata;
            DriveInfoQueries.GetVolumeLabel(_name, metadata.LabelBuffer, out int length);
            return metadata.LabelBuffer.AsSpan(0, length);
        }
    }

    /// <summary>Gets available free space in bytes.</summary>
    public long AvailableFreeSpace => DriveInfoQueries.GetAvailableFreeSpace(_name);

    /// <summary>Gets total free space in bytes.</summary>
    public long TotalFreeSpace => DriveInfoQueries.GetTotalFreeSpace(_name);

    /// <summary>Gets total drive size in bytes.</summary>
    public long TotalSize => DriveInfoQueries.GetTotalSize(_name);

    /// <summary>Sets the volume label.</summary>
    public void SetVolumeLabel(ReadOnlySpan<char> label) {
        if (label.Contains('\0')) {
            throw SpanIOException.NullCharacterInPath(nameof(label));
        }

        DriveInfoQueries.SetVolumeLabel(_name, label);
    }

    /// <summary>Enumerates logical drives into a caller-provided buffer.</summary>
    public static SpanDriveEnumerator EnumerateDrives(Span<char> nameBuffer) => new(nameBuffer);

    /// <summary>Gets logical drive names into caller-provided buffers.</summary>
    public static bool TryGetDrives(Span<char> namesBuffer, Span<int> nameLengths, out int driveCount) {
        driveCount = 0;
        int offset = 0;
        using IDriveEnumerator enumerator = NativePlatformTable.InvokeEnumerateDrives();
        while (enumerator.MoveNext()) {
            if (driveCount >= nameLengths.Length) {
                return false;
            }

            if (offset > namesBuffer.Length) {
                return false;
            }

            Span<char> destination = namesBuffer[offset..];
            if (!enumerator.TryGetCurrentDrive(destination, out int length) || offset + length > namesBuffer.Length) {
                return false;
            }

            nameLengths[driveCount] = length;
            offset += length;
            driveCount++;
        }

        return true;
    }

    /// <summary>Gets logical drive names, throwing when buffers are too small.</summary>
    public static int GetDrives(Span<char> namesBuffer, Span<int> nameLengths) =>
        TryGetDrives(namesBuffer, nameLengths, out int count)
            ? count
            : throw SpanIOException.DestinationTooSmall();

    private DriveInfoData Metadata => _metadata ??= new DriveInfoData();
}

/// <summary>
/// Enumerates logical drives into a caller-provided buffer.
/// </summary>
/// <remarks>
/// The name buffer must remain valid until this enumerator is disposed.
/// Each <see cref="Current"/> name is valid only until the next <c>MoveNext</c> or <c>foreach</c> iteration.
/// </remarks>
public ref struct SpanDriveEnumerator {
    private IDriveEnumerator? _enumerator;
    private Span<char> _nameBuffer;
    private int _currentLength;

    internal SpanDriveEnumerator(Span<char> nameBuffer) {
        _enumerator = NativePlatformTable.InvokeEnumerateDrives();
        _nameBuffer = nameBuffer;
        _currentLength = 0;
    }

    /// <summary>Gets the current drive name.</summary>
    public ReadOnlySpan<char> Current => _nameBuffer[.._currentLength];

    /// <summary>Enables <c>foreach</c> over this enumerator.</summary>
    public readonly SpanDriveEnumerator GetEnumerator() => this;

    /// <summary>Advances to the next drive.</summary>
    public bool MoveNext() {
        if (_enumerator is null) {
            return false;
        }

        while (_enumerator.MoveNext()) {
            if (_enumerator.TryGetCurrentDrive(_nameBuffer, out _currentLength)) {
                return true;
            }

            return false;
        }

        _currentLength = 0;
        return false;
    }

    /// <summary>Releases enumeration resources.</summary>
    public void Dispose() => _enumerator?.Dispose();
}
