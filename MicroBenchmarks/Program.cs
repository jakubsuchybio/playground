// See https://aka.ms/new-console-template for more information

using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.Portable;
using Windows.Devices.Usb;
using BenchmarkDotNet.Running;

//BenchmarkRunner.Run<MicroBenchmarks.Benchmarks>();

var allDevices = await DeviceInformation.FindAllAsync();
var holter802Devices = allDevices.Where(x => x.Name.Contains("802-")).ToArray();
var usbholter802Devices = holter802Devices.Where(x => x.Id.Contains("USB#")).ToArray();

/*var usbholter802Device = usbholter802Devices!.First();
//var usbDevice = await UsbDevice.FromIdAsync(usbholter802Device.Id);

// parase vid and pid from: {Id: \\?\USB#VID_0424&PID_2240#000000000000#{a5dcbf10-6530-11d2-901f-00c04fb951ed}}
var parts = usbholter802Device.Id.Split('#')[1].Split('&');
var vid = parts[0].Split('_')[1];
var pid = parts[1].Split('_')[1];
var parsedVid = uint.Parse(vid, System.Globalization.NumberStyles.HexNumber);
var parsedPid = uint.Parse(pid, System.Globalization.NumberStyles.HexNumber);
*/
uint vid = 0x0424;
uint pid = 0x2240;
var guid = new Guid("36fc9e60-c465-11cf-8056-444553540000");

var aqs = UsbDevice.GetDeviceSelector(vid, pid, guid);
var usbDevice = DeviceInformation.FindAllAsync(aqs, null).GetAwaiter().GetResult().ToArray();
UsbDevice usbDevice2;

foreach (var device in holter802Devices)
{
    try
    {
        usbDevice2 = await UsbDevice.FromIdAsync(device.Id);
        break;
    }
    catch
    {
        // ignored
    }
}

Console.WriteLine($"Device name");