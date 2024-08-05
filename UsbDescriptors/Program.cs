using System;
using UsbDescriptors.FinalForm;

namespace UsbDescriptors
{
    public static class Program
    {
        public static void Main()
        {
            var descriptorsArray = PublicApi.GetAllHolter802DevicesWithDescriptors();
            foreach (var descriptors in descriptorsArray)
                Console.WriteLine($"Product: {descriptors.Product}; Manufacturer: {descriptors.Manufacturer}; SerialNumber: {descriptors.SerialNumber};");
        }
    }
}