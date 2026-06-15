namespace FileSystemics.IO.Internal;

internal enum DriveEnumerationKind {
    WindowsLogicalDrives,
    LinuxMounts,
    MacMounts,
    UnixSingleRoot,
}
