namespace FileSystemics.IO.Internal;

internal sealed class MacOSDrivePlatform : INativeDrivePlatform<MacOSDrivePlatform> {
    private MacOSDrivePlatform() {
    }

    public static DriveEnumerationKind EnumerationKind => DriveEnumerationKind.MacMounts;

    public static bool UsesDriveLetters => false;

    public static bool VolumeLabelIsMountPath => true;

    public static int GetNormalizedDriveCapacity(ReadOnlySpan<char> driveName, string paramName) {
        DriveNameNormalizer.Validate(driveName, paramName);
        return driveName.Length;
    }

    public static int NormalizeDriveName(ReadOnlySpan<char> driveName, Span<char> destination, string paramName) {
        int capacity = GetNormalizedDriveCapacity(driveName, paramName);
        if (destination.Length < capacity) {
            throw SpanIOException.DestinationTooSmall();
        }

        driveName.CopyTo(destination);
        return driveName.Length;
    }

    public static DriveType GetDriveType(ReadOnlySpan<char> name) => MacOSDriveInfo.GetDriveType(name);

    public static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
        MacOSDriveInfo.GetDriveFormat(name, destination, out charsWritten);

    public static void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        if (name.Length > destination.Length) {
            throw SpanIOException.DestinationTooSmall();
        }

        name.CopyTo(destination);
        charsWritten = name.Length;
    }

    public static void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) =>
        throw new PlatformNotSupportedException();

    public static long GetAvailableFreeSpace(ReadOnlySpan<char> name) => MacOSDriveInfo.GetAvailableFreeSpace(name);

    public static long GetTotalFreeSpace(ReadOnlySpan<char> name) => MacOSDriveInfo.GetTotalFreeSpace(name);

    public static long GetTotalSize(ReadOnlySpan<char> name) => MacOSDriveInfo.GetTotalSize(name);

    public static IDriveEnumerator EnumerateDrives() => new LogicalDriveEnumeratorCore();
}
