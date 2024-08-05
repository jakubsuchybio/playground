using System;
using System.Collections.Generic;
using System.Linq;

namespace UsbDescriptors.FinalForm;

public class UsbDescriptors
{
    public string Product { get; }
    public string Manufacturer { get; }
    public string SerialNumber { get; }
    
    public UsbDescriptors(string product, string manufacturer, string serialNumber)
    {
        Product = product;
        Manufacturer = manufacturer;
        SerialNumber = serialNumber;
    }
    
    public static UsbDescriptors Empty => new(string.Empty, string.Empty, string.Empty);
}

public static class PublicApi
{
    public static IReadOnlyCollection<UsbDescriptors> GetAllHolter802DevicesWithDescriptors()
    {
        var holterDevices = InternalApi.GetAllUsbDevices()
            .Where(x => x.Key.Contains("VID_0424&PID_2240"))
            .ToDictionary(x => x.Key, x => x.Value);
        
        if(holterDevices.Count == 0)
        {
            Console.WriteLine("No Holter 802 devices found.");
            return Array.Empty<UsbDescriptors>();
        }
        
        foreach (var device in holterDevices)
        {
            Console.WriteLine("Device: " + device.Key);
            Console.WriteLine("    in hub Device: " + device.Value);
        }

        return holterDevices.Select(x => InternalApi.GetDeviceDescriptors(x.Value, x.Key)).ToArray();
    }
}
