# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2026-06-09

### Added

- **FileSystemics.SpanIO** — span-first alternatives to common `System.IO` path and file APIs using `ReadOnlySpan<char>` parameters.
- Public APIs: `SpanPath`, `SpanFile`, `SpanDirectory`, `SpanFileInfo`, `SpanDirectoryInfo`, `SpanDriveInfo`, `SpanBuilder`.
- Native platform dispatch for Linux, macOS, and Windows (UTF-8 on Unix, UTF-16 on Windows).
- Platform-aware full-path normalization for relative-path resolution without `System.IO.Path.GetFullPath`.
- **FileSystemics.SpanIO.Platform** — swappable `IPlatformHost` backends via `Platform.Use`, `Platform.Register`, and `FILESYSTEMICS_PLATFORM`.
- Cross-platform test suite (TUnit) and CI workflow (Ubuntu, Windows, macOS, .NET 10).
- BenchmarkDotNet micro-benchmarks with interactive runner and BCL baselines.

[0.5.0]: https://github.com/slinkynexus/FileSystemics/releases/tag/v0.5.0
