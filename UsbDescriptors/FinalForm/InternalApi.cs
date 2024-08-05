using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UsbDescriptors.FinalForm;

public static class InternalApi
{
    private const int CR_SUCCESS = 0;
    private const int USB_STRING_DESCRIPTOR_TYPE = 3;


    private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
    private const uint FILE_DEVICE_USB = FILE_DEVICE_UNKNOWN;
    private const uint METHOD_BUFFERED = 0;
    private const uint FILE_ANY_ACCESS = 0;

    private const uint USB_GET_NODE_CONNECTION_INFORMATION_EX = 274;

    private static readonly uint IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX =
        GetCtlCode(FILE_DEVICE_USB, USB_GET_NODE_CONNECTION_INFORMATION_EX, METHOD_BUFFERED, FILE_ANY_ACCESS);

    private const uint USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 260;

    private static readonly uint IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION =
        GetCtlCode(FILE_DEVICE_USB, USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, METHOD_BUFFERED, FILE_ANY_ACCESS);


    private static uint GetCtlCode(uint deviceType, uint function, uint method, uint access) =>
        ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);

    internal static Dictionary<string, string> GetAllUsbDevices()
    {
        var result = new Dictionary<string, string>();

        var hubsInterfaceGuid = new Guid("F18A0E88-C30C-11D0-8815-00A0C906BED8");
        const int DIGCF_PRESENT = 0x00000002;
        const int DIGCF_DEVICEINTERFACE = 0x00000010;
        var deviceInfoSet = Interop.SetupDiGetClassDevs(ref hubsInterfaceGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
        if (deviceInfoSet == IntPtr.Zero)
        {
            Console.WriteLine("Error getting device info set"); // TODO change to log
            return result;
        }

        try
        {
            // Enumerates all devices with the 'hubs' interface GUID
            var deviceInfoData = new SP_DEVINFO_DATA();
            for (uint i = 0; Interop.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                var deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
                if (!Interop.SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref hubsInterfaceGuid, i, ref deviceInterfaceData))
                {
                    Console.WriteLine("Error getting device interface data"); // TODO change to log
                    continue;
                }

                var requiredSizeDetail = 0;
                Interop.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref requiredSizeDetail, ref deviceInfoData);
                var detailDataBuffer = Marshal.AllocHGlobal(requiredSizeDetail);

                try
                {
                    var deviceInterfaceDetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA_UNMANAGED();
                    Marshal.StructureToPtr(deviceInterfaceDetailData, detailDataBuffer, false);

                    if (!Interop.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, detailDataBuffer, requiredSizeDetail, ref requiredSizeDetail, ref deviceInfoData))
                    {
                        Console.WriteLine("Error getting the hub data"); // TODO change to log
                        continue;
                    }

                    Marshal.PtrToStructure<SP_DEVICE_INTERFACE_DETAIL_DATA_UNMANAGED>(detailDataBuffer);
                    var pDevicePath = IntPtr.Add(detailDataBuffer, IntPtr.Size);
                    var parentPath = Marshal.PtrToStringAuto(pDevicePath);

                    // First child is by CM_Get_Child
                    if (Interop.CM_Get_Child(out var childDevInst, deviceInfoData.DevInst, 0) != CR_SUCCESS)
                        continue;

                    var childDeviceId = new StringBuilder(256);
                    if (Interop.CM_Get_Device_ID(childDevInst, childDeviceId, childDeviceId.Capacity, 0) != CR_SUCCESS)
                    {
                        Console.WriteLine("Error getting the device id of a child"); // TODO change to log
                        continue;
                    }

                    result.Add(childDeviceId.ToString(), parentPath);

                    // Another children is by CM_Get_Sibling
                    while (Interop.CM_Get_Sibling(out childDevInst, childDevInst, 0) == CR_SUCCESS)
                    {
                        var siblingDeviceId = new StringBuilder(256);
                        if (Interop.CM_Get_Device_ID(childDevInst, siblingDeviceId, siblingDeviceId.Capacity, 0) != CR_SUCCESS)
                        {
                            Console.WriteLine("Error getting the device id of a sibling"); // TODO change to log
                            continue;
                        }

                        result.Add(siblingDeviceId.ToString(), parentPath);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(detailDataBuffer);
                }
            }
        }
        finally

        {
            Interop.SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }

        return result;
    }

    internal static UsbDescriptors GetDeviceDescriptors(string parentPath, string deviceId)
    {
        if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(deviceId))
            return UsbDescriptors.Empty;

        if (Interop.CM_Locate_DevNodeA(out var deviceInst, deviceId, 0) != CR_SUCCESS)
        {
            Console.WriteLine($"Could not get device instance handle for '{deviceId}'");
            return UsbDescriptors.Empty;
        }

        var props = new USB_DEVICE_PROPS();
        var size = (uint)Marshal.SizeOf(props.port);
        const uint CM_DRP_ADDRESS = 29;
        if (Interop.CM_Get_DevNode_Registry_PropertyA(deviceInst, CM_DRP_ADDRESS, IntPtr.Zero, ref props.port, ref size, 0) != CR_SUCCESS)
        {
            Console.WriteLine($"Could not get port for '{deviceId}'");
            return UsbDescriptors.Empty;
        }

        const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        var handle = Interop.CreateFileA(
            parentPath,
            (uint)FILE_ACCESS_FLAGS.GENERIC_WRITE,
            (uint)FILE_SHARE.WRITE,
            IntPtr.Zero,
            (uint)CreateDisposition.OPEN_EXISTING,
            FILE_FLAG_OVERLAPPED, IntPtr.Zero);
        if (handle == IntPtr.Zero)
        {
            Console.WriteLine($"Could not open hub {parentPath}: {Marshal.GetLastWin32Error()}");
            return UsbDescriptors.Empty;
        }

        try
        {
            var connInfo = new USB_NODE_CONNECTION_INFORMATION_EX();
            size = (uint)Marshal.SizeOf(connInfo);
            connInfo.ConnectionIndex = props.port;
            var connInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(connInfo));
            try
            {
                Marshal.StructureToPtr(connInfo, connInfoPtr, false);

                if (!Interop.DeviceIoControl(handle, IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX, connInfoPtr, size, connInfoPtr, size, out size, IntPtr.Zero))
                {
                    Console.WriteLine($"Could not get node connection information for '{deviceId}': {Marshal.GetLastWin32Error()}");
                    return UsbDescriptors.Empty;
                }

                connInfo = Marshal.PtrToStructure<USB_NODE_CONNECTION_INFORMATION_EX>(connInfoPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(connInfoPtr);
            }

            var product = connInfo.DeviceDescriptor.iProduct != 0
                ? GetUsbStringDescriptor(handle, props.port, connInfo.DeviceDescriptor.iProduct, 0x0409)
                : string.Empty;
            var manufacturer = connInfo.DeviceDescriptor.iManufacturer != 0
                ? GetUsbStringDescriptor(handle, props.port, connInfo.DeviceDescriptor.iManufacturer, 0x0409)
                : string.Empty;
            var serialNumber = connInfo.DeviceDescriptor.iSerialNumber != 0
                ? GetUsbStringDescriptor(handle, props.port, connInfo.DeviceDescriptor.iSerialNumber, 0x0409)
                : string.Empty;

            return new UsbDescriptors(product, manufacturer, serialNumber);
        }
        finally
        {
            Interop.CloseHandle(handle);
        }
    }

    private static string GetUsbStringDescriptor(IntPtr deviceHandle, uint port, byte index, ushort languageId)
    {
        var descriptorRequest = new USB_DESCRIPTOR_REQUEST
        {
            ConnectionIndex = port,
            SetupPacket = new USB_SETUP_PACKET
            {
                bmRequest = 0x80, // Device-to-host, standard, device
                bRequest = 0x06, // GET_DESCRIPTOR
                wValue = (ushort)((USB_STRING_DESCRIPTOR_TYPE << 8) | index),
                wIndex = languageId,
                wLength = (ushort)Marshal.SizeOf<USB_STRING_DESCRIPTOR>()
            }
        };

        var bufferSize = Marshal.SizeOf<USB_DESCRIPTOR_REQUEST>() + Marshal.SizeOf<USB_STRING_DESCRIPTOR>();
        var buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            Marshal.StructureToPtr(descriptorRequest, buffer, false);
            var stringDescriptorPtr = IntPtr.Add(buffer, Marshal.SizeOf<USB_DESCRIPTOR_REQUEST>());
            Marshal.StructureToPtr(new USB_STRING_DESCRIPTOR(), stringDescriptorPtr, false);

            var result = Interop.DeviceIoControl(
                deviceHandle,
                IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION,
                buffer,
                (uint)bufferSize,
                buffer,
                (uint)bufferSize,
                out _,
                IntPtr.Zero
            );

            if (!result)
            {
                Console.WriteLine($"Failed to retrieve string descriptor. Error code: {Marshal.GetLastWin32Error()}");
                return null;
            }

            var stringDescriptor = Marshal.PtrToStructure<USB_STRING_DESCRIPTOR>(stringDescriptorPtr);
            return new string(stringDescriptor.bString, 0, stringDescriptor.bLength - 2);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}