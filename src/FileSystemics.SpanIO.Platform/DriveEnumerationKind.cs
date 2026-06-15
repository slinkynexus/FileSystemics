namespace FileSystemics.IO;

/// <summary>
/// Describes how logical drives are enumerated for a platform host.
/// </summary>
public enum DriveEnumerationKind {
    /// <summary>Windows logical drive bitmask enumeration.</summary>
    WindowsLogicalDrives,

    /// <summary>Linux mount table enumeration.</summary>
    LinuxMounts,

    /// <summary>macOS mount table enumeration.</summary>
    MacMounts,

    /// <summary>Unix single-root enumeration.</summary>
    UnixSingleRoot,
}
