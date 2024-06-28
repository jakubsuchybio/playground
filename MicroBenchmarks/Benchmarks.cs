using BenchmarkDotNet.Attributes;
using Windows.Devices.Enumeration;

namespace MicroBenchmarks;

public class Benchmarks
{
    [Benchmark]
    public async Task FindAllDevices()
    {
        var devices = await DeviceInformation.FindAllAsync();
    }
    
    [Benchmark]
    public void FindAllDrives()
    {
        var drives = DriveInfo.GetDrives();
    }
}