namespace FileSystemics.Abstractions;

/// <summary>
/// Watches a directory for changes mirroring <see cref="FileSystemWatcher"/> (Phase 2).
/// </summary>
public interface ISpanFileSystemWatcher : IDisposable;
