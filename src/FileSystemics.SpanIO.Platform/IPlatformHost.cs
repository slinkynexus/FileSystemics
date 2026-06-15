namespace FileSystemics.IO;

/// <summary>
/// Bundles filesystem, path, and drive behavior for a single platform.
/// </summary>
public interface IPlatformHost {
    /// <summary>File and directory operations for this host.</summary>
    IFileSystemPlatform FileSystem { get; }

    /// <summary>Path rules and native encoding for this host.</summary>
    IPathRules Path { get; }

    /// <summary>Drive naming, enumeration, and space queries for this host.</summary>
    IDrivePlatform Drives { get; }
}
