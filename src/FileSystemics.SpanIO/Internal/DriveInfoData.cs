namespace FileSystemics.IO.Internal;

internal sealed class DriveInfoData {
    internal const int MetadataBufferLength = 261;

    internal char[] FormatBuffer { get; } = new char[MetadataBufferLength];

    internal char[] LabelBuffer { get; } = new char[MetadataBufferLength];
}
