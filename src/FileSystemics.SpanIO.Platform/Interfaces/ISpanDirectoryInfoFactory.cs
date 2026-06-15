using System.Diagnostics.CodeAnalysis;

namespace FileSystemics.Abstractions;

/// <summary>
/// Factory for <see cref="ISpanDirectoryInfo"/> instances.
/// </summary>
public interface ISpanDirectoryInfoFactory : ISpanFileSystemEntity {
    /// <summary>Creates directory metadata for the specified path.</summary>
    ISpanDirectoryInfo New(ReadOnlySpan<char> path);

    /// <summary>Wraps an existing <see cref="DirectoryInfo"/>.</summary>
    [return: NotNullIfNotNull(nameof(directoryInfo))]
    ISpanDirectoryInfo? Wrap(DirectoryInfo? directoryInfo);
}
