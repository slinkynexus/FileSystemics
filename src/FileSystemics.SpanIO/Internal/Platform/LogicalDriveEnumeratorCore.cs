using System.Buffers;
using System.Runtime.InteropServices;
using FileSystemics.IO.Interop;

namespace FileSystemics.IO.Internal;

internal sealed class LogicalDriveEnumeratorCore : IDriveEnumerator {
    private const int UnixMountBufferLength = 4096;

    private readonly DriveEnumerationKind _enumerationKind;

    private uint _windowsDriveBitmask;
    private char _windowsDriveLetter = 'A';
    private char _currentWindowsLetter;
    private bool _windowsInitialized;
    private bool _windowsFinished;

    private FileStream? _mountsStream;
    private byte[]? _mountsByteBuffer;
    private int _mountsByteLength;
    private int _mountsBytePosition;
    private bool _unixSingleRootYielded;

    private nint _macMountsBuffer;
    private int _macMountsCount;
    private int _macMountsIndex;
    private bool _macMountsInitialized;

    private readonly char[] _currentUnixMount = new char[UnixMountBufferLength];
    private int _currentUnixMountLength;

    internal LogicalDriveEnumeratorCore() {
        _enumerationKind = NativePlatformTable.DriveEnumerationKind;
    }

    public bool TryGetCurrentDrive(Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        if (_enumerationKind == DriveEnumerationKind.WindowsLogicalDrives) {
            return TryGetCurrentWindowsDrive(destination, out charsWritten);
        }

        return TryGetCurrentUnixDrive(destination, out charsWritten);
    }

    public bool MoveNext() {
        if (_enumerationKind == DriveEnumerationKind.WindowsLogicalDrives) {
            return MoveNextWindows();
        }

        return MoveNextUnix();
    }

    public void Dispose() {
        _mountsStream?.Dispose();
        _mountsStream = null;
        if (_mountsByteBuffer is not null) {
            ArrayPool<byte>.Shared.Return(_mountsByteBuffer);
            _mountsByteBuffer = null;
        }
    }

    private bool MoveNextWindows() {
        if (!_windowsInitialized) {
            _windowsDriveBitmask = InteropWindowsDrive.GetLogicalDrives();
            if (_windowsDriveBitmask == 0) {
                InteropHelpers.ThrowExceptionForLastError();
            }

            _windowsInitialized = true;
            _windowsDriveLetter = 'A';
        }

        if (_windowsFinished) {
            return false;
        }

        while (_windowsDriveBitmask != 0) {
            if ((_windowsDriveBitmask & 1) != 0) {
                _currentWindowsLetter = _windowsDriveLetter;
                _windowsDriveBitmask >>= 1;
                _windowsDriveLetter++;
                return true;
            }

            _windowsDriveBitmask >>= 1;
            _windowsDriveLetter++;
        }

        _windowsFinished = true;
        return false;
    }

    private bool TryGetCurrentWindowsDrive(Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        if (destination.Length < 3) {
            return false;
        }

        destination[0] = _currentWindowsLetter;
        destination[1] = ':';
        destination[2] = '\\';
        charsWritten = 3;
        return true;
    }

    private bool MoveNextUnix() {
        if (_enumerationKind == DriveEnumerationKind.UnixSingleRoot) {
            if (_unixSingleRootYielded) {
                return false;
            }

            _unixSingleRootYielded = true;
            _currentUnixMount[0] = '/';
            _currentUnixMountLength = 1;
            return true;
        }

        if (_enumerationKind == DriveEnumerationKind.MacMounts) {
            return MoveNextMacMounts();
        }

        EnsureUnixMountStream();
        while (TryReadNextUnixMountPoint()) {
            return true;
        }

        return false;
    }

    private bool TryGetCurrentUnixDrive(Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        if (_currentUnixMountLength > destination.Length) {
            return false;
        }

        _currentUnixMount.AsSpan(0, _currentUnixMountLength).CopyTo(destination);
        charsWritten = _currentUnixMountLength;
        return true;
    }

    private bool MoveNextMacMounts() {
        if (!_macMountsInitialized) {
            _macMountsCount = InteropMacOs.GetMountEntries(out _macMountsBuffer);
            _macMountsInitialized = true;
            if (_macMountsCount <= 0) {
                return false;
            }
        }

        while (_macMountsIndex < _macMountsCount) {
            nint entryAddress = _macMountsBuffer + (_macMountsIndex * InteropMacOs.StatfsSize);
            _macMountsIndex++;
            InteropMacOs.Statfs entry = Marshal.PtrToStructure<InteropMacOs.Statfs>(entryAddress);
            ReadOnlySpan<char> mountPoint = entry.f_mntonname.AsSpan().TrimEnd('\0');
            if (mountPoint.IsEmpty) {
                continue;
            }
            if (mountPoint.Length > _currentUnixMount.Length) {
                continue;
            }

            mountPoint.CopyTo(_currentUnixMount);
            _currentUnixMountLength = mountPoint.Length;
            return true;
        }

        return false;
    }

    private void EnsureUnixMountStream() {
        if (_mountsStream is not null) {
            return;
        }

        _mountsStream = File.OpenRead("/proc/mounts");
        _mountsByteBuffer = ArrayPool<byte>.Shared.Rent(4096);
        _mountsByteLength = 0;
        _mountsBytePosition = 0;
    }

    private bool TryReadNextUnixMountPoint() {
        Span<byte> lineBuffer = stackalloc byte[1024];
        while (TryReadMountLine(lineBuffer, out int lineLength)) {
            if (LinuxMountEntryParser.TryGetMountPoint(
                    lineBuffer[..lineLength],
                    _currentUnixMount,
                    out _currentUnixMountLength)) {
                return true;
            }
        }

        _currentUnixMountLength = 0;
        return false;
    }

    private bool TryReadMountLine(Span<byte> lineBuffer, out int lineLength) {
        lineLength = 0;
        while (true) {
            if (_mountsBytePosition >= _mountsByteLength) {
                if (_mountsStream is null || _mountsByteBuffer is null) {
                    return false;
                }

                int read = _mountsStream.Read(_mountsByteBuffer, 0, _mountsByteBuffer.Length);
                if (read == 0) {
                    return lineLength > 0;
                }

                _mountsByteLength = read;
                _mountsBytePosition = 0;
            }

            byte value = _mountsByteBuffer![_mountsBytePosition++];
            if (value == (byte)'\n') {
                return lineLength > 0;
            }

            if (lineLength < lineBuffer.Length) {
                lineBuffer[lineLength++] = value;
            }
        }
    }
}
