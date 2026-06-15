using BenchmarkDotNet.Running;

namespace FileSystemics.SpanIO.Benchmarks;

internal static class Program {
    private static void Main(string[] args) {
        if (args.Length == 0) {
            args = BenchmarkMenu.ResolveArgs();
            if (args.Length == 0) {
                return;
            }
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

internal static class BenchmarkMenu {
    private static readonly (string Number, string Label, string[] Args)[] Options = [
        ("1", "Timing suite (all *Timing* benchmarks, latency + BCL baselines)", ["--filter", "*Timing*"]),
        ("2", "Allocation suite (all *Allocation* benchmarks, memory profiling)", ["--filter", "*Allocation*"]),
        ("3", "SpanPath (join, combine, relative path, extension)", ["--filter", "*SpanPath*"]),
        ("4", "SpanFile (exists, attributes, read/write)", ["--filter", "*SpanFile*"]),
        ("5", "SpanDirectory (exists, get files, enumerate file paths)", ["--filter", "*SpanDirectory*"]),
        ("6", "SpanDrive (drive type, enumeration)", ["--filter", "*SpanDrive*"]),
        ("7", "SpanInfo (SpanFileInfo, SpanDirectoryInfo, SpanBuilder)", ["--filter", "*SpanInfo*"]),
        ("8", "All benchmarks", ["--filter", "*"]),
        ("9", "List every benchmark method (--list flat)", ["--list", "flat"]),
        ("0", "Exit", []),
    ];

    internal static string[] ResolveArgs() {
        PrintHeader();

        foreach ((string number, string label, _) in Options) {
            Console.WriteLine($"  {number}  {label}");
        }

        PrintExamples();
        Console.Write("Enter choice [0-9, default 1]: ");

        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input)) {
            input = "1";
        }

        if (input.StartsWith("--", StringComparison.Ordinal)) {
            return input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        foreach ((string number, string label, string[] args) in Options) {
            if (string.Equals(input, number, StringComparison.Ordinal)) {
                if (args.Length == 0) {
                    Console.WriteLine("Exiting.");
                }

                return args;
            }
        }

        if (input.Contains('*', StringComparison.Ordinal)) {
            return ["--filter", input];
        }

        return ["--filter", $"*{input}*"];
    }

    private static void PrintHeader() {
        Console.WriteLine();
        Console.WriteLine("FileSystemics SpanIO Benchmarks");
        Console.WriteLine("===============================");
        Console.WriteLine();
        Console.WriteLine("Select a benchmark suite:");
        Console.WriteLine();
    }

    private static void PrintExamples() {
        Console.WriteLine();
        Console.WriteLine("Non-interactive examples (pass args after --):");
        Console.WriteLine("  dotnet run -c Release --project benchmark/FileSystemics.SpanIO.Benchmarks -- --filter *Timing*");
        Console.WriteLine("  dotnet run -c Release --project benchmark/FileSystemics.SpanIO.Benchmarks -- --filter *SpanPathTiming*");
        Console.WriteLine("  dotnet run -c Release --project benchmark/FileSystemics.SpanIO.Benchmarks -- SpanPathTimingBenchmarks");
        Console.WriteLine("  dotnet run -c Release --project benchmark/FileSystemics.SpanIO.Benchmarks -- --list flat");
        Console.WriteLine();
        Console.WriteLine("You can also type a class name (e.g. SpanPathTimingBenchmarks) or a filter (e.g. *SpanFile*).");
        Console.WriteLine();
    }
}
