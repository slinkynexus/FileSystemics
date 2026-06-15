namespace FileSystemics.IO;

/// <summary>
/// Convenience base type for custom <see cref="IPlatformHost"/> implementations.
/// </summary>
public abstract class PlatformHost : IPlatformHost {
    /// <inheritdoc/>
    public abstract IFileSystemPlatform FileSystem { get; }

    /// <inheritdoc/>
    public abstract IPathRules Path { get; }

    /// <inheritdoc/>
    public abstract IDrivePlatform Drives { get; }
}
