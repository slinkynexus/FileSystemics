# FileSystemics: Span based IO and Abstractions

Span-first `System.IO` alternatives for .NET. Path parameters are `ReadOnlySpan<char>` so hot paths can avoid string allocations and unnecessary garbage collection while still calling native filesystem APIs.

## About

`System.IO` is built around strings. Strings for names, strings for extensions, combinations of strings for paths and formatted file names. Fine for most, but this can become expensive in very big data applications with high memory pressure. When hot paths cause GC churn in multithreaded systems everything slows down.

FileSystemics was created for those workloads — log sinks, render and asset pipelines, scanners, build tools, sync agents, and test harnesses that touch many files per second. The core idea is simple: keep paths as `ReadOnlySpan<char>` through join, normalize, enumerate, and open, and let the caller supply `Span<char>` scratch space (or rent a `char[]` when paths exceed the stack threshold) instead of forcing intermediate strings on every call.

This bypasses a lot of the BCL internal logic and there are other micro-performance tradeoffs. We're trading some nanoseconds for memory stability across a complex system, which should improve overall speed. Caveat artifex.

## Packages

Core span-based path, file, directory, and drive APIs (zero dependencies)

```shell
dotnet add package FileSystemics.SpanIO
```

Abstractions, mockables, custom platforms.

```shell
dotnet add package FileSystemics.SpanIO.Platform
```

## Abstraction layers

Three entry points serve different concerns; pick the narrowest one for your use case:


| Layer                                        | When to use                                                         |
| -------------------------------------------- | ------------------------------------------------------------------- |
| `SpanPath` / `SpanFile` / `SpanDirectory`    | Hot paths, libraries, zero DI — static APIs on `ReadOnlySpan<char>` |
| `ISpanFileSystem` (`SpanFileSystem.Default`) | App and test code with dependency injection and mocks               |
| `IPlatformHost` / `Platform.Use`             | Custom path rules or native I/O backends (orthogonal to DI)         |


`SpanFileSystem.Default` still respects `Platform.Use` because adapters delegate to static `Span`* APIs.

## Usage Examples

### Static APIs (`SpanPath`, `SpanFile`, `SpanDirectory`)

#### File-based logging

High-volume loggers should not `Path.Combine` on every write. Join at sink construction, open the handle once, and append each `ReadOnlySpan<char>` line without transcoding — the in-memory `char` span is written as UTF-16LE bytes via `MemoryMarshal.AsBytes`.

```csharp
using System.Runtime.InteropServices;
using FileSystemics.IO;
using Microsoft.Win32.SafeHandles;

public sealed class FileLogSink : IDisposable {
    private readonly SafeFileHandle _handle;
    private long _offset;

    public FileLogSink(ReadOnlySpan<char> logDir) {
        Span<char> logPath = stackalloc char[logDir.Length + 8];
        SpanPath.TryJoin(logDir, "app.log".AsSpan(), logPath, out int length);
        _handle = SpanFile.OpenHandle(
            logPath[..length], FileMode.Append, FileAccess.Write, FileShare.Read);
        _offset = RandomAccess.GetLength(_handle);
    }

    public void Write(ReadOnlySpan<char> line) {
        ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(line);
        RandomAccess.Write(_handle, bytes, _offset);
        _offset += bytes.Length;
    }

    public void Write(string line) => Write(line.AsSpan());

    public void Dispose() => _handle.Dispose();
}
```

**Alternatively:**

```csharp
public sealed class TextLogSink : IDisposable {
    private readonly StreamWriter _writer;

    public TextLogSink(ReadOnlySpan<char> logDir) {
        Span<char> logPath = stackalloc char[logDir.Length + 8];
        SpanPath.TryJoin(logDir, "app.log".AsSpan(), logPath, out int length);
        _writer = SpanFile.AppendText(logPath[..length]);
    }

    public void Write(ReadOnlySpan<char> line) => _writer.Write(line);

    public void Write(string line) => _writer.WriteLine(line);

    public void Dispose() => _writer.Dispose();
}
```

#### Log retention

Nightly jobs delete or compress rotated logs. Walk the directory as spans; filter on the file-name prefix before touching disk metadata.

```csharp
ReadOnlySpan<char> logDir = settings.LogDirectory.AsSpan();
ReadOnlySpan<char> pattern = "*.log".AsSpan();
ReadOnlySpan<char> rotatedPrefix = "app-".AsSpan();
Span<char> buffer = stackalloc char[SpanDirectory.GetEntryPathBufferCapacity(logDir)];

foreach (ReadOnlySpan<char> path in SpanDirectory.EnumerateFiles(logDir, buffer, pattern)) {
    ReadOnlySpan<char> name = SpanPath.GetFileName(path);
    if (name.StartsWith(rotatedPrefix) && SpanFile.Exists(path)) {
        CompressOrDelete(path);
    }
}
```

#### Render-farm frame paths

Compositor output paths share a fixed shape; only the frame index changes.

```csharp
ReadOnlySpan<char> outputDir = job.OutputDirectory.AsSpan();
ReadOnlySpan<char> fileName = "frame_000000.exr".AsSpan();
Span<char> framePath = stackalloc char[outputDir.Length + fileName.Length + 1];

SpanPath.TryJoin(outputDir, fileName, framePath, out int pathLength);
int digitOffset = pathLength - 10; // six digits immediately before ".exr"

for (int frame = 0; frame < job.FrameCount; frame++) {
    frame.TryFormat(framePath[digitOffset..(digitOffset + 6)], out _, format: "D6");
    using FileStream output = SpanFile.OpenWrite(framePath[..pathLength]);
    WriteFrame(output, frame);
}
```

#### Texture catalog scan

A watch folder ingests new textures. Enumeration yields spans; parsing stays allocation-free until you hit a cache or GPU API that needs `string`.

```csharp
ReadOnlySpan<char> watchDir = catalog.InboxDirectory.AsSpan();
ReadOnlySpan<char> pattern = "*.png".AsSpan();
ReadOnlySpan<char> pngExt = ".png".AsSpan();
Span<char> buffer = stackalloc char[SpanDirectory.GetEntryPathBufferCapacity(watchDir)];

foreach (ReadOnlySpan<char> path in SpanDirectory.EnumerateFiles(watchDir, buffer, pattern)) {
    if (!SpanPath.GetExtension(path).Equals(pngExt, StringComparison.OrdinalIgnoreCase)) {
        continue;
    }

    ReadOnlySpan<char> stem = SpanPath.GetFileNameWithoutExtension(path);
    if (catalog.TryGetThumbnail(stem, out Thumbnail thumb)) {
        QueueImport(path, thumb);
    }
}
```

### Abstraction layer (`ISpanFileSystem`)

App and test code inject `ISpanFileSystem` (`SpanFileSystem.Default` in production, fakes in tests). Path, file, and directory operations go through `fs.Path`, `fs.File`, and `fs.Directory`.

#### Log archiver

Archive rotated logs without `Path.Combine` per entry. `fs.Directory.EnumerateFiles` walks matches into a caller buffer; copy through `fs.File` so tests can substitute the filesystem.

```csharp
using FileSystemics.Abstractions;

public sealed class LogArchiver(ISpanFileSystem fs) {
    public void Archive(ReadOnlySpan<char> logDir, ReadOnlySpan<char> archiveDir) {
        ReadOnlySpan<char> pattern = "*.log".AsSpan();
        Span<char> entry = stackalloc char[fs.Directory.GetEntryPathBufferCapacity(logDir)];
        Span<char> dest = stackalloc char[fs.Directory.GetEntryPathBufferCapacity(archiveDir)];

        foreach (ReadOnlySpan<char> path in fs.Directory.EnumerateFiles(logDir, entry, pattern)) {
            ReadOnlySpan<char> name = fs.Path.GetFileName(path);
            fs.Path.TryJoin(archiveDir, name, dest, out int destLength);
            fs.File.Copy(path, dest[..destLength], overwrite: false);
        }
    }
}
```

Same zero-allocation walk as static `SpanDirectory.EnumerateFiles`, routed through `ISpanDirectory` for dependency injection.

### Custom platform hosts

Reference `FileSystemics.SpanIO.Platform` when you need to override path rules or filesystem behavior (tests, plugins, cross-platform simulation). An `IPlatformHost` bundles three surfaces: `FileSystem`, `Path`, and `Drives`.

The simplest host is the OS default — no wrapper required:

```csharp
IPlatformHost myHost = Platform.Actual;
```

To change path semantics while keeping real OS file I/O, derive from `PlatformHost` and delegate `FileSystem` / `Drives` to `Platform.Actual`:

```csharp
using FileSystemics.IO;

IPlatformHost myHost = new CustomPathHost(Platform.Actual);

using (Platform.Use(myHost)) {
    // SpanPath, SpanFile, SpanDirectory, and native dispatch use myHost for this scope
    ReadOnlySpan<char> dir = SpanPath.GetDirectoryName(@"dir\subdir\file.txt".AsSpan());
}

sealed class CustomPathHost(IPlatformHost inner) : PlatformHost {
    // Implement IFileSystemPlatform (or wrap inner.Path) to change how IO commands are executed
    public override IFileSystemPlatform FileSystem => inner.FileSystem;
    // Implement IDrivePlatform (or wrap inner.Path) to change drive letters, information etc.
    public override IDrivePlatform Drives => inner.Drives;
    // Implement IPathRules (or wrap inner.Path) to change separators, roots, encoding, etc.
    public override IPathRules Path { get; } = new MyPathRules(inner.Path);
}

```

When rebinding separators on an OS that uses different native rules (e.g. backslash paths on Linux), override `GetRootLength` and `IsPathRooted` as well — see `test/FileSystemics.SpanIO.Platform.Tests/MockPlatform/ForwardSlashPathHost.cs`.

Register a named host for process-wide selection via the `FILESYSTEMICS_PLATFORM` environment variable:

```csharp
Platform.Register("backslash", () => new BackslashPathHost(Platform.Actual));
// FILESYSTEMICS_PLATFORM=backslash
```

`Platform.Use` scopes overrides and restores the previous host on dispose; nested scopes are supported.

## Contributing

Issues and pull requests are welcome, give me your feedback.

## Support

If FileSystemics is useful to you, or if you're morally and philosophically opposed to unnecessary heap allocation, donations are highly appreciated and help fund development/research and support:

[GitHub Sponsors](https://github.com/sponsors/slinkynexus) · [Patreon](https://www.patreon.com/slinkynexus) · [PayPal](https://paypal.me/slinkynexus)