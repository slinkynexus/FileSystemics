using System.Diagnostics.CodeAnalysis;

namespace FileSystemics.Abstractions;

/// <summary>
/// Factory for <see cref="ISpanFileInfo"/> instances.
/// </summary>
public interface ISpanFileInfoFactory : ISpanFileSystemEntity {
    /// <summary>Creates file metadata for the specified path.</summary>
    ISpanFileInfo New(ReadOnlySpan<char> fileName);

    /// <summary>Wraps an existing <see cref="FileInfo"/>.</summary>
    [return: NotNullIfNotNull(nameof(fileInfo))]
    ISpanFileInfo? Wrap(FileInfo? fileInfo);
}
