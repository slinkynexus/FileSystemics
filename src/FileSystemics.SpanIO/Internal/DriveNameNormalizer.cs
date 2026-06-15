namespace FileSystemics.IO.Internal;

internal static class DriveNameNormalizer {
    internal static int GetNormalizedCapacity(ReadOnlySpan<char> driveName, string paramName) =>
        NativePlatformTable.InvokeGetNormalizedDriveCapacity(driveName, paramName);

    internal static int Normalize(ReadOnlySpan<char> driveName, Span<char> destination, string paramName) =>
        NativePlatformTable.InvokeNormalizeDriveName(driveName, destination, paramName);

    internal static void Validate(ReadOnlySpan<char> driveName, string paramName) {
        if (driveName.IsEmpty) {
            throw SpanIOException.MustBeNonEmptyDriveName(paramName);
        }

        if (driveName.Contains('\0')) {
            throw SpanIOException.InvalidDriveCharacters(paramName);
        }
    }
}
