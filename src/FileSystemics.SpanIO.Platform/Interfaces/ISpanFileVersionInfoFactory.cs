namespace FileSystemics.Abstractions;

/// <summary>
/// Factory for <see cref="ISpanFileVersionInfo"/> instances.
/// </summary>
public interface ISpanFileVersionInfoFactory : ISpanFileSystemEntity {
    /// <summary>Returns version information for the specified file.</summary>
    ISpanFileVersionInfo GetVersionInfo(ReadOnlySpan<char> fileName);
}
