using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;

namespace FileSystemics.SpanIO.Benchmarks.Timing;

[Config(typeof(MicroTimingConfig))]
public class SpanDriveTimingBenchmarks {
    private readonly char[] _nameBuffer = new char[64];
    private string _driveName = "";

    [GlobalSetup]
    public void Setup() {
        _driveName = OperatingSystem.IsWindows() ? @"C:\" : "/";
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public DriveType SpanDriveInfo_DriveType() {
        SpanDriveInfo drive = new(_driveName.AsSpan());
        return drive.DriveType;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BCL")]
    public DriveType Bcl_DriveType() {
        DriveInfo drive = new(_driveName);
        return drive.DriveType;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanDriveEnumerator_MoveNext() {
        SpanDriveEnumerator enumerator = SpanDriveInfo.EnumerateDrives(_nameBuffer);
        return enumerator.MoveNext();
    }
}
