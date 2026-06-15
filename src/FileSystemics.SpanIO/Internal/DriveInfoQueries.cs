namespace FileSystemics.IO.Internal;

internal static class DriveInfoQueries {
    internal static DriveType GetDriveType(ReadOnlySpan<char> name) =>
        NativePlatformTable.InvokeGetDriveType(name);

    internal static void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
        NativePlatformTable.InvokeGetDriveFormat(name, destination, out charsWritten);

    internal static void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten) =>
        NativePlatformTable.InvokeGetVolumeLabel(name, destination, out charsWritten);

    internal static void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label) =>
        NativePlatformTable.InvokeSetVolumeLabel(name, label);

    internal static long GetAvailableFreeSpace(ReadOnlySpan<char> name) =>
        NativePlatformTable.InvokeGetAvailableFreeSpace(name);

    internal static long GetTotalFreeSpace(ReadOnlySpan<char> name) =>
        NativePlatformTable.InvokeGetTotalFreeSpace(name);

    internal static long GetTotalSize(ReadOnlySpan<char> name) =>
        NativePlatformTable.InvokeGetTotalSize(name);
}
