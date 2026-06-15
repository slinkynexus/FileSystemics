using BenchmarkDotNet.Attributes;
using FileSystemics.IO;
using FileSystemics.SpanIO.Benchmarks.Configs;

namespace FileSystemics.SpanIO.Benchmarks.Allocation;

[Config(typeof(AllocationConfig))]
public class SpanDriveAllocationBenchmarks {
    private readonly char[] _namesBuffer = new char[512];
    private readonly int[] _nameLengths = new int[32];
    private string _driveName = "";

    [GlobalSetup]
    public void Setup() {
        _driveName = OperatingSystem.IsWindows() ? @"C:\" : "/";
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public ReadOnlySpan<char> SpanDriveInfo_Name() {
        SpanDriveInfo drive = new(_driveName.AsSpan());
        return drive.Name;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public DriveType SpanDriveInfo_DriveType() {
        SpanDriveInfo drive = new(_driveName.AsSpan());
        return drive.DriveType;
    }

    [Benchmark]
    [BenchmarkCategory("SpanIO")]
    public bool SpanDriveInfo_TryGetDrives() =>
        SpanDriveInfo.TryGetDrives(_namesBuffer, _nameLengths, out _);

    [Benchmark]
    [BenchmarkCategory("BCL")]
    public DriveInfo[] Bcl_GetDrives() =>
        DriveInfo.GetDrives();
}
