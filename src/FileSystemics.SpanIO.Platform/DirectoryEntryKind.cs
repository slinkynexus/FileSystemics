namespace FileSystemics.IO;

/// <summary>
/// Filters entries returned by <see cref="IFileSystemPlatform.EnumerateDirectory"/>.
/// </summary>
public enum DirectoryEntryKind {
    /// <summary>Files only.</summary>
    Files,

    /// <summary>Directories only.</summary>
    Directories,

    /// <summary>Files and directories.</summary>
    All,
}
