using System.Diagnostics.CodeAnalysis;

namespace FileSystemics.Abstractions;

/// <summary>
/// Factory for <see cref="ISpanDriveInfo"/> instances.
/// </summary>
public interface ISpanDriveInfoFactory : ISpanFileSystemEntity {
    /// <summary>Returns logical drives on the computer.</summary>
    ISpanDriveInfo[] GetDrives();

    /// <summary>Provides access to information on the specified drive.</summary>
    ISpanDriveInfo New(ReadOnlySpan<char> driveName);

    /// <summary>Wraps an existing <see cref="DriveInfo"/>.</summary>
    [return: NotNullIfNotNull(nameof(driveInfo))]
    ISpanDriveInfo? Wrap(DriveInfo? driveInfo);
}
