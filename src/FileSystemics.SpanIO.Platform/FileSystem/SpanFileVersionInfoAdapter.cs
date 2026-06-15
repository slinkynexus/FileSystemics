using System.Diagnostics;
using FileSystemics.Abstractions;
using FileSystemics.IO.Internal;

namespace FileSystemics.IO;

internal sealed class SpanFileVersionInfoAdapter : ISpanFileVersionInfo {
    private readonly OwnedSpanPath _fileName;
    private readonly FileVersionInfo _versionInfo;

    internal SpanFileVersionInfoAdapter(ReadOnlySpan<char> fileName) {
        PathArgumentValidation.ValidatePath(fileName, nameof(fileName));
        _fileName = new OwnedSpanPath(fileName);
        _versionInfo = FileVersionInfo.GetVersionInfo(fileName.ToString());
    }

    public ReadOnlySpan<char> FileName => _fileName.Span;

    public string? FileVersion => _versionInfo.FileVersion;

    public string? ProductVersion => _versionInfo.ProductVersion;

    public string? CompanyName => _versionInfo.CompanyName;

    public string? FileDescription => _versionInfo.FileDescription;

    public string? ProductName => _versionInfo.ProductName;

    public string? OriginalFilename => _versionInfo.OriginalFilename;

    public string? InternalName => _versionInfo.InternalName;

    public bool IsDebug => _versionInfo.IsDebug;

    public bool IsPatched => _versionInfo.IsPatched;

    public bool IsPrivateBuild => _versionInfo.IsPrivateBuild;

    public bool IsPreRelease => _versionInfo.IsPreRelease;

    public bool IsSpecialBuild => _versionInfo.IsSpecialBuild;
}
