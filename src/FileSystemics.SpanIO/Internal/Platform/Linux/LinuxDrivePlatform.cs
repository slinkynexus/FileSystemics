namespace FileSystemics.IO.Internal;

internal sealed class LinuxDrivePlatform : INativeDrivePlatform<LinuxDrivePlatform> {
    private LinuxDrivePlatform() {
    }

    public static DriveEnumerationKind EnumerationKind => DriveEnumerationKind.LinuxMounts;

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

    public static DriveType GetDriveType(ReadOnlySpan<char> name) => LinuxDriveInfo.GetDriveType(name);

    public static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
        LinuxDriveInfo.GetDriveFormat(name, destination, out charsWritten);

    public static void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) {
        if (name.Length > destination.Length) {
            throw SpanIOException.DestinationTooSmall();
        }

        name.CopyTo(destination);
        charsWritten = name.Length;
    }

    public static void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) =>
        throw new PlatformNotSupportedException();

    public static long GetAvailableFreeSpace(ReadOnlySpan<char> name) => LinuxDriveInfo.GetAvailableFreeSpace(name);

    public static long GetTotalFreeSpace(ReadOnlySpan<char> name) => LinuxDriveInfo.GetTotalFreeSpace(name);

    public static long GetTotalSize(ReadOnlySpan<char> name) => LinuxDriveInfo.GetTotalSize(name);

    public static IDriveEnumerator EnumerateDrives() => new LogicalDriveEnumeratorCore();
}
