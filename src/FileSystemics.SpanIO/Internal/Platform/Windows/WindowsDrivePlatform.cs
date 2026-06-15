namespace FileSystemics.IO.Internal;

internal sealed class WindowsDrivePlatform : INativeDrivePlatform<WindowsDrivePlatform> {
    private WindowsDrivePlatform() {
    }

    public static DriveEnumerationKind EnumerationKind => DriveEnumerationKind.WindowsLogicalDrives;

    public static bool UsesDriveLetters => true;

    public static bool VolumeLabelIsMountPath => false;

    public static int GetNormalizedDriveCapacity(ReadOnlySpan<char> driveName, string paramName) {
        DriveNameNormalizer.Validate(driveName, paramName);
        if (driveName.Length == 1) {
            return 3;
        }

        int rootLength = WindowsPathRules.GetRootLength(driveName);
        ReadOnlySpan<char> root = rootLength == 0 ? ReadOnlySpan<char>.Empty : driveName[..rootLength];
        if (root.IsEmpty || root.StartsWith("\\\\")) {
            throw SpanIOException.MustBeDriveLetterOrRootDirectory(paramName);
        }

        return root.Length + (root.Length == 2 && root[1] == ':' ? 1 : 0);
    }

    public static int NormalizeDriveName(ReadOnlySpan<char> driveName, Span<char> destination, string paramName) {
        int capacity = GetNormalizedDriveCapacity(driveName, paramName);
        if (destination.Length < capacity) {
            throw SpanIOException.DestinationTooSmall();
        }

        if (driveName.Length == 1) {
            if (!IsValidDriveChar(driveName[0])) {
                throw SpanIOException.MustBeDriveLetterOrRootDirectory(paramName);
            }

            destination[0] = driveName[0];
            destination[1] = ':';
            destination[2] = '\\';
            return 3;
        }

        int rootLength = WindowsPathRules.GetRootLength(driveName);
        ReadOnlySpan<char> root = driveName[..rootLength];
        root.CopyTo(destination);
        int length = root.Length;
        if (length == 2 && destination[1] == ':') {
            destination[length++] = '\\';
        }

        return length;
    }

    public static DriveType GetDriveType(ReadOnlySpan<char> name) => WindowsDriveInfo.GetDriveType(name);

    public static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
        WindowsDriveInfo.GetDriveFormat(name, destination, out charsWritten);

    public static void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
        WindowsDriveInfo.GetVolumeLabel(name, destination, out charsWritten);

    public static void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) =>
        WindowsDriveInfo.SetVolumeLabel(name, label);

    public static long GetAvailableFreeSpace(ReadOnlySpan<char> name) => WindowsDriveInfo.GetAvailableFreeSpace(name);

    public static long GetTotalFreeSpace(ReadOnlySpan<char> name) => WindowsDriveInfo.GetTotalFreeSpace(name);

    public static long GetTotalSize(ReadOnlySpan<char> name) => WindowsDriveInfo.GetTotalSize(name);

    public static IDriveEnumerator EnumerateDrives() => new LogicalDriveEnumeratorCore();

    private static bool IsValidDriveChar(char value) =>
        (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
}
