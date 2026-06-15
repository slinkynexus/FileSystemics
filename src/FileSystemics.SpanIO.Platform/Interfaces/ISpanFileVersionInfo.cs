namespace FileSystemics.Abstractions;

/// <summary>
/// File version information mirroring <see cref="System.Diagnostics.FileVersionInfo"/>.
/// </summary>
public interface ISpanFileVersionInfo {
    /// <summary>Gets the file path the version information describes.</summary>
    ReadOnlySpan<char> FileName { get; }

    /// <summary>Gets the file version string.</summary>
    string? FileVersion { get; }

    /// <summary>Gets the product version string.</summary>
    string? ProductVersion { get; }

    /// <summary>Gets the company name.</summary>
    string? CompanyName { get; }

    /// <summary>Gets the file description.</summary>
    string? FileDescription { get; }

    /// <summary>Gets the product name.</summary>
    string? ProductName { get; }

    /// <summary>Gets the original file name.</summary>
    string? OriginalFilename { get; }

    /// <summary>Gets the internal name.</summary>
    string? InternalName { get; }

    /// <summary>Gets whether the file is a debug build.</summary>
    bool IsDebug { get; }

    /// <summary>Gets whether the file has been patched.</summary>
    bool IsPatched { get; }

    /// <summary>Gets whether the file is a private build.</summary>
    bool IsPrivateBuild { get; }

    /// <summary>Gets whether the file is a pre-release build.</summary>
    bool IsPreRelease { get; }

    /// <summary>Gets whether the file is a special build.</summary>
    bool IsSpecialBuild { get; }
}
