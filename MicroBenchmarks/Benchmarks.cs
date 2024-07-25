using BenchmarkDotNet.Attributes;
using Windows.Devices.Enumeration;

namespace MicroBenchmarks;

public class Benchmarks
{
    [Benchmark(Baseline = true)]
    public async Task FindStorageDevices()
    {
        var devices = await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);
    }
    
    [Benchmark]
    public async Task FindAllDevices()
    {
        var devices = await DeviceInformation.FindAllAsync();
    }
}