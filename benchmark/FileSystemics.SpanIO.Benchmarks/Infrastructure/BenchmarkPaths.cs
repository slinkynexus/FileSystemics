namespace FileSystemics.SpanIO.Benchmarks.Infrastructure;

public enum PathSize {
    Short,
    Medium,
    Long,
}

internal static class BenchmarkPaths {
    internal const string ShortRelative = "alpha/beta/gamma.txt";

    internal const string MediumRelative =
        "projects/filesystemics/src/components/widgets/forms/validation/rules/messages/en-US/templates/default.txt";

    internal static readonly string LongRelative = $"{string.Concat(
        Enumerable.Repeat("segment/", 64))}target.dat";

    internal static ReadOnlySpan<char> GetRelative(PathSize size) => size switch {
        PathSize.Short => ShortRelative.AsSpan(),
        PathSize.Medium => MediumRelative.AsSpan(),
        PathSize.Long => LongRelative.AsSpan(),
        _ => throw new ArgumentOutOfRangeException(nameof(size)),
    };

    internal static string GetRelativeString(PathSize size) => size switch {
        PathSize.Short => ShortRelative,
        PathSize.Medium => MediumRelative,
        PathSize.Long => LongRelative,
        _ => throw new ArgumentOutOfRangeException(nameof(size)),
    };

    internal const int DestinationCapacity = 1024;
}
