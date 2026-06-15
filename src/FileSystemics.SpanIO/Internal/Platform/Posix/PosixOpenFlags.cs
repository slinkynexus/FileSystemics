namespace FileSystemics.IO.Internal;

/// <summary>
/// Platform-specific <c>open(2)</c> flag values. Linux and macOS use different bit masks for <c>O_CREAT</c>, <c>O_EXCL</c>, and <c>O_TRUNC</c>.
/// </summary>
internal static class PosixOpenFlags {
    internal const int O_RDONLY = 0;
    internal const int O_WRONLY = 1;
    internal const int O_RDWR = 2;

    internal static int Creat => OperatingSystem.IsMacOS() ? 0x200 : 0x40;
    internal static int Excl => OperatingSystem.IsMacOS() ? 0x800 : 0x80;
    internal static int Trunc => OperatingSystem.IsMacOS() ? 0x400 : 0x200;
}
