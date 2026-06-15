namespace FileSystemics.IO.Internal;

internal interface INativeDrivePlatform<T> where T : INativeDrivePlatform<T> {
    static abstract DriveEnumerationKind EnumerationKind { get; }

    static abstract bool UsesDriveLetters { get; }

    static abstract bool VolumeLabelIsMountPath { get; }

    static abstract int GetNormalizedDriveCapacity(ReadOnlySpan<char> driveName, string paramName);

    static abstract int NormalizeDriveName(ReadOnlySpan<char> driveName, Span<char> destination, string paramName);

    static abstract DriveType GetDriveType(ReadOnlySpan<char> name);

    static abstract void GetDriveFormat(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten);

    static abstract void GetVolumeLabel(ReadOnlySpan<char> name, Span<char> destination, out int charsWritten);

    static abstract void SetVolumeLabel(ReadOnlySpan<char> name, ReadOnlySpan<char> label);

    static abstract long GetAvailableFreeSpace(ReadOnlySpan<char> name);

    static abstract long GetTotalFreeSpace(ReadOnlySpan<char> name);

    static abstract long GetTotalSize(ReadOnlySpan<char> name);

    static abstract IDriveEnumerator EnumerateDrives();
}
