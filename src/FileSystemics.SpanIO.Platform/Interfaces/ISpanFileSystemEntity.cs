namespace FileSystemics.Abstractions;

/// <summary>
/// Associates a filesystem abstraction member with its root <see cref="ISpanFileSystem"/>.
/// </summary>
public interface ISpanFileSystemEntity {
    /// <summary>The root filesystem abstraction.</summary>
    ISpanFileSystem FileSystem { get; }
}
