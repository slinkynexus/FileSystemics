namespace FileSystemics.SpanIO.Benchmarks.Infrastructure;

public sealed class BenchmarkFileSystem : IDisposable {
    internal string TempRoot { get; private set; } = "";

    internal string ExistingDirectory { get; private set; } = "";

    internal string ExistingFile { get; private set; } = "";

    internal string MissingFile { get; private set; } = "";

    internal void Create() {
        TempRoot = Path.Combine(Path.GetTempPath(), $"filesystemics-bench-{Guid.NewGuid():N}");
        Directory.CreateDirectory(TempRoot);

        ExistingDirectory = Path.Combine(TempRoot, "data");
        Directory.CreateDirectory(ExistingDirectory);

        ExistingFile = Path.Combine(ExistingDirectory, "bench.txt");
        File.WriteAllText(ExistingFile, new string('x', 4096));

        MissingFile = Path.Combine(ExistingDirectory, "missing.txt");
    }

    public void Dispose() {
        if (TempRoot.Length > 0 && Directory.Exists(TempRoot)) {
            Directory.Delete(TempRoot, recursive: true);
        }
    }
}
