using System.Runtime.InteropServices;
using System.Text;
using FileSystemics.IO.Interop;

namespace FileSystemics.IO.Internal;

internal sealed unsafe class FileSystemEntryEnumerator : IFileSystemDirectoryEnumerator {
    private const int GetdentsBufferSize = 8192;
    private const int MaxEntryNameUtf8Bytes = DirectoryEntryPathReader.MaxEntryNameChars;

    private int _dirFd;
    private nint _dirHandle;
    private bool _disposed;
    private readonly DirectoryEntryKind _kind;
    private int _bufferOffset;
    private int _bufferCount;
    private int _entryNameByteLength;
    private bool _hasCurrentEntry;
    private readonly bool _useGetdents64;
#pragma warning disable CS0414, CS0169 // Accessed through fixed-buffer member pins.
    private NativeBuffers _buffers;
#pragma warning restore CS0414, CS0169

    internal FileSystemEntryEnumerator(
        ReadOnlySpan<char> directoryPath,
        ReadOnlySpan<char> searchPattern,
        DirectoryEntryKind kind) {
        // Search pattern is applied by SpanDirectoryEntryEnumerator on Unix.
        _ = searchPattern;
        _kind = kind;
        _dirFd = -1;
        _dirHandle = nint.Zero;
        _bufferOffset = 0;
        _bufferCount = 0;
        _useGetdents64 = NativePlatformTable.UsesGetdents64DirectoryEnumeration;

        if (_useGetdents64) {
            _dirFd = OpenDirectoryFd(directoryPath);
        }
        else {
            _dirHandle = OpenDirectoryHandle(directoryPath);
        }
    }

    public bool IsDirectory { get; private set; }

    public bool TryGetCurrentEntryName(Span<char> destination, out int charsWritten) {
        charsWritten = 0;
        if (!_hasCurrentEntry) {
            return false;
        }

        fixed (byte* namePtr = _buffers.EntryNameUtf8) {
            ReadOnlySpan<byte> nameBytes = new(namePtr, _entryNameByteLength);
            int charCount = Encoding.UTF8.GetCharCount(nameBytes);
            if (charCount > destination.Length) {
                return false;
            }

            charsWritten = Encoding.UTF8.GetChars(nameBytes, destination);
        }

        return true;
    }

    public bool MoveNext() => !_disposed && MoveNextCore();

    public void Dispose() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        _hasCurrentEntry = false;
        if (_dirFd >= 0) {
            InteropUnix.close(_dirFd);
            _dirFd = -1;
        }

        if (_dirHandle != nint.Zero) {
            InteropUnixDir.closedir(_dirHandle);
            _dirHandle = nint.Zero;
        }
    }

    private bool MoveNextCore() =>
        _useGetdents64 ? MoveNextLinux() : MoveNextPosixReaddir();

    private bool MoveNextLinux() {
        while (true) {
            if (_bufferOffset >= _bufferCount) {
                fixed (byte* bufferPtr = _buffers.GetdentsBuffer) {
                    int read = InteropUnix.getdents64(_dirFd, bufferPtr, GetdentsBufferSize);
                    if (read <= 0) {
                        _hasCurrentEntry = false;
                        return false;
                    }

                    _bufferOffset = 0;
                    _bufferCount = read;
                }
            }

            fixed (byte* bufferPtr = _buffers.GetdentsBuffer) {
                ReadOnlySpan<byte> buffer = new(bufferPtr, _bufferCount);
                int recordOffset = _bufferOffset;
                ushort dReclen = BitConverter.ToUInt16(buffer.Slice(recordOffset + 16, 2));
                byte dType = buffer[recordOffset + 18];
                int nameOffset = recordOffset + 19;
                int nameLength = 0;
                while (nameOffset + nameLength < recordOffset + dReclen && buffer[nameOffset + nameLength] != 0) {
                    nameLength++;
                }

                ReadOnlySpan<byte> nameBytes = buffer.Slice(nameOffset, nameLength);
                _bufferOffset += dReclen;

                if (TryAcceptEntry(nameBytes, dType, out bool isDirectory)) {
                    IsDirectory = isDirectory;
                    return true;
                }
            }
        }
    }

    private bool MoveNextPosixReaddir() {
        while (true) {
            nint entryPtr = InteropUnixDir.readdir(_dirHandle);
            if (entryPtr == nint.Zero) {
                _hasCurrentEntry = false;
                return false;
            }

            byte* entry = (byte*)entryPtr;
            byte dType = entry[20];
            ReadOnlySpan<byte> nameBytes = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(entry + 21);

            if (TryAcceptEntry(nameBytes, dType, out bool isDirectory)) {
                IsDirectory = isDirectory;
                return true;
            }
        }
    }

    private bool TryAcceptEntry(ReadOnlySpan<byte> nameBytes, byte dType, out bool isDirectory) {
        isDirectory = false;
        _hasCurrentEntry = false;
        if (nameBytes.SequenceEqual("."u8) || nameBytes.SequenceEqual(".."u8)) {
            return false;
        }

        bool isDir = dType == InteropConstants.DT_DIR;
        bool isFile = dType == InteropConstants.DT_REG || dType == 0;
        if (isDir && _kind == DirectoryEntryKind.Files) {
            return false;
        }

        if (!isDir && _kind == DirectoryEntryKind.Directories) {
            return false;
        }

        if (!isDir && !isFile && dType != 0) {
            return false;
        }

        if (nameBytes.Length > MaxEntryNameUtf8Bytes) {
            return false;
        }

        fixed (byte* namePtr = _buffers.EntryNameUtf8) {
            nameBytes.CopyTo(new Span<byte>(namePtr, MaxEntryNameUtf8Bytes));
        }

        _entryNameByteLength = nameBytes.Length;
        isDirectory = isDir;
        _hasCurrentEntry = true;
        return true;
    }

    private static int OpenDirectoryFd(ReadOnlySpan<char> directoryPath) =>
        PlatformPath.WithNativePath(
            directoryPath,
            (_, utf8Path) => {
                int opened = InteropUnix.open(utf8Path, InteropConstants.O_RDONLY, 0);
                if (opened < 0) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return opened;
            });

    private static nint OpenDirectoryHandle(ReadOnlySpan<char> directoryPath) =>
        PlatformPath.WithNativePath(
            directoryPath,
            (_, utf8Path) => {
                nint opened = InteropUnixDir.opendir(utf8Path);
                if (opened == nint.Zero) {
                    InteropHelpers.ThrowExceptionForErrno(Marshal.GetLastWin32Error());
                }

                return opened;
            });

    private struct NativeBuffers {
        public fixed byte GetdentsBuffer[GetdentsBufferSize];
        public fixed byte EntryNameUtf8[MaxEntryNameUtf8Bytes];
    }
}
