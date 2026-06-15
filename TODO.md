# FileSystemics TODO

Tracked follow-ups for SpanIO and platform work. See [docs/platform-hierarchy.png](docs/platform-hierarchy.png) for the call stack from public APIs to native interop.

## UNC path handling

Windows already parses `\\server\share\...` roots in `[WindowsPathRules.GetRootLength](src/FileSystemics.SpanIO/Internal/Platform/Windows/WindowsPathRules.cs)`. A dedicated **UNC platform** would make those semantics available as a named host (on any OS) and close BCL parity gaps.

### Goals

- Add built-in `UncPathHost` (or `UncPlatformHost`) in `FileSystemics.SpanIO.Platform`
- Register via `Platform.Register("unc", () => new UncPathHost(Platform.Actual))`
- Support `FILESYSTEMICS_PLATFORM=unc` for process-wide selection
- Document UNC usage in README alongside other custom hosts

### Path rules (`IPathRules`)

- Reuse or extract Windows-style `GetRootLength` for `\\server\share` (stop after server + share segments)
- Implement BCL-accurate `IsPartiallyQualified` (not just `GetRootLength == 0`):
  - `\\server\share\file` — fully qualified
  - `\share\file` — current-drive rooted (expand with cwd drive root)
  - `\\?\UNC\server\share\...` -- extended device path
- Treat `\` and `/` as separators; `UsesUtf16NativePaths = true`
- `PathComparison.OrdinalIgnoreCase` for UNC server/share segments
- Add tests: root length, `GetPathRoot`, `TryGetRelativePath`, `TryGetFullPath` for UNC and `\\?\UNC\` prefixes
- Expand `[SpanPathTests](test/FileSystemics.SpanIO.Tests/SpanPathTests.cs)` beyond the single `\\server\share\folder\file.txt` `GetDirectoryName` case

### File I/O strategy

- **Phase 1 (recommended):** delegate `IFileSystemPlatform` to `Platform.Actual` — real UNC I/O on Windows via Win32; path APIs use UNC rules everywhere
- **Phase 2 (optional):** SMB client backend for Linux/macOS (out of scope for initial UNC host)

### Drive platform (`IDrivePlatform`)

- `UsesDriveLetters = false`
- Allow `NormalizeDriveName` for `\\server\share` roots (today `[WindowsDrivePlatform](src/FileSystemics.SpanIO/Internal/Platform/Windows/WindowsDrivePlatform.cs)` rejects `\\` prefixes)
- `GetDriveType` / free-space queries against UNC roots on Windows
- Decide enumeration model: network share list vs. no enumeration

### Related fixes (Windows native, not a separate host)

- Align `WindowsPathRules.IsPartiallyQualified` with BCL `PathInternal` (drive-relative `C:foo`, current-drive `\foo`)
- `SpanPathFullPath`: extended `\\?\` and device `\\.\` paths on custom hosts
- Benchmark UNC path normalization vs. `System.IO.Path`

---

## Custom platform examples

Custom hosts implement `[IPlatformHost](src/FileSystemics.SpanIO.Platform/IPlatformHost.cs)` (or subclass `[PlatformHost](src/FileSystemics.SpanIO.Platform/PlatformHost.cs)`) with three surfaces: `Path`, `FileSystem`, `Drives`.

### Examples to add

- `UncPathHost` -- production-ready UNC path rules (see above)
- `InMemoryFileSystemHost` -- `IFileSystemPlatform` backed by a dictionary (unit tests, no disk)
- `DefaultFileSystem` -- wrapper for the default `System.IO.`* static classes
- `ReadOnlyFileSystemHost` -- wrapper that throws on write/delete (sandboxed tools)
- `LoggingFileSystemHost` -- decorator that logs calls then forwards to inner `FileSystem`
- `RootedFileSystem` -- wrapper that locks the filesystem to a set jail directory
- `HybridFileSystem` -- wrapper that selectively uses different PlatformHosts internally
- **Sample app** under `examples/CustomPlatform/` showing:
  ```csharp
  Platform.Register("backslash", () => new BackslashPathHost(Platform.Actual));
  using (Platform.Use(new BackslashPathHost(Platform.Actual))) { ... }
  ```

### Registration patterns

```csharp
// Startup (process-wide via environment variable)
Platform.Register("unc", () => new UncPathHost(Platform.Actual));
// export FILESYSTEMICS_PLATFORM=unc

// Scoped override (restored on dispose; supports nesting)
using (Platform.Use(myHost)) {
    SpanPath.Join(...);
    SpanFile.Exists(...);
}

// Inspect active host
IPlatformHost current = Platform.Current;  // override or Platform.Actual
```

### Implementation checklist for a new host

1. Implement `IPathRules` (or wrap/delegate to `Platform.Actual.Path`)
2. Choose `IFileSystemPlatform` — delegate to OS, or custom backend
3. Choose `IDrivePlatform` — delegate or stub unsupported operations
4. Expose via `PlatformHost` subclass
5. Register and add tests under `test/FileSystemics.SpanIO.Platform.Tests/`
6. Mark tests `[NotInParallel("platform-binding")]` when mutating global bindings

---

## Abstractions layer

Two NuGet packages: `FileSystemics.SpanIO` (core) and `FileSystemics.SpanIO.Platform` (hosts + DI). Interfaces (`ISpan*`) ship in Platform under the `FileSystemics.Abstractions` namespace; `SpanFileSystem.Default` provides the default adapters.

Implemented:

- `ISpanFileSystem` with path/file/directory facades and factory members
- Class adapters (`SpanFileInfoAdapter`, `SpanDirectoryInfoAdapter`, `SpanDriveInfoAdapter`) delegating to `Span*` ref structs; navigation routes through factories for mockability
- `ISpanFileVersionInfo` backed by `FileVersionInfo.GetVersionInfo`
- Thin path/file/directory facades collapsed into `SpanFileSystem` nested types

### Follow-ups

- `ISpanDrive` static facade (if needed alongside `ISpanDriveInfo`)
- `ISpanDirectoryEntryEnumerator` aligned with `IFileSystemDirectoryEnumerator`
- `ISpanFileSystemWatcher` surface (events/properties beyond `IDisposable`)
- `FileSystemics.SpanIO.Platform.Testing` package with in-memory mocks (see custom platform examples)
- README: full DI registration examples per host framework

