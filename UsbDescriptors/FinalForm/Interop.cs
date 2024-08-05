using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UsbDescriptors.FinalForm;

internal static class Interop
{
    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);
    
    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);
    
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, uint MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
    public static extern int CM_Get_Child(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
    public static extern int CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, int BufferLen, uint ulFlags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
    public static extern int CM_Get_Sibling(out uint pdnDevInst, uint dnDevInst, uint ulFlags);
    
    [DllImport("cfgmgr32.dll", SetLastError = true)]
    public static extern uint CM_Locate_DevNodeA(out uint pdnDevInst, string pDeviceID, uint ulFlags);
    
    [DllImport("cfgmgr32.dll", SetLastError = true)]
    public static extern uint CM_Get_DevNode_Registry_PropertyA(uint dnDevInst, uint ulProperty, IntPtr pulRegDataType, ref uint Buffer, ref uint pulLength, uint ulFlags);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);
}


[StructLayout(LayoutKind.Sequential)]
public struct SP_DEVINFO_DATA
{
    public readonly int cbSize;
    public Guid ClassGuid;
    public uint DevInst;
    public IntPtr Reserved;

    public SP_DEVINFO_DATA()
    {
        cbSize = Marshal.SizeOf(this);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct SP_DEVICE_INTERFACE_DATA
{
    public readonly int cbSize;
    public Guid InterfaceClassGuid;
    public int Flags;
    public IntPtr Reserved;

    public SP_DEVICE_INTERFACE_DATA()
    {
        cbSize = Marshal.SizeOf(this);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct SP_DEVICE_INTERFACE_DETAIL_DATA_UNMANAGED
{
    public readonly int cbSize;
    public IntPtr DevicePath; // Pointer to the device path string

    public SP_DEVICE_INTERFACE_DETAIL_DATA_UNMANAGED()
    {
        cbSize = IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize; // Adjust for 64-bit or 32-bit
    }
}

[StructLayout(LayoutKind.Sequential)]
struct USB_DEVICE_PROPS
{
    public uint vid;
    public uint pid;
    public uint speed;
    public uint lower_speed;
    public uint port;
    public bool is_USB;
    public bool is_SCSI;
    public bool is_CARD;
    public bool is_UASP;
    public bool is_VHD;
    public bool is_Removable;
}

[StructLayout(LayoutKind.Sequential)]
public struct USB_NODE_CONNECTION_INFORMATION_EX
{
    public uint ConnectionIndex;
    public USB_DEVICE_DESCRIPTOR DeviceDescriptor;
    public byte CurrentConfigurationValue;
    public byte Speed;
    public byte DeviceIsHub;
    public ushort DeviceAddress;
    public uint NumberOfOpenPipes;
    public USB_PIPE_INFO[] PipeList;
}

[StructLayout(LayoutKind.Sequential)]
public struct USB_DEVICE_DESCRIPTOR
{
    public byte bLength;
    public byte bDescriptorType;
    public ushort bcdUSB;
    public byte bDeviceClass;
    public byte bDeviceSubClass;
    public byte bDeviceProtocol;
    public byte bMaxPacketSize0;
    public ushort idVendor;
    public ushort idProduct;
    public ushort bcdDevice;
    public byte iManufacturer;
    public byte iProduct;
    public byte iSerialNumber;
    public byte bNumConfigurations;
}

[StructLayout(LayoutKind.Sequential)]
public struct USB_PIPE_INFO
{
    public USB_ENDPOINT_DESCRIPTOR EndpointDescriptor;
    public uint ScheduleOffset;
}

[StructLayout(LayoutKind.Sequential)]
public struct USB_ENDPOINT_DESCRIPTOR
{
    public byte bLength;
    public byte bDescriptorType;
    public byte bEndpointAddress;
    public byte bmAttributes;
    public ushort wMaxPacketSize;
    public byte bInterval;
}

[StructLayout(LayoutKind.Sequential)]
public struct USB_DESCRIPTOR_REQUEST
{
    public uint ConnectionIndex;
    public USB_SETUP_PACKET SetupPacket;
}

[StructLayout(LayoutKind.Sequential)]
public struct USB_SETUP_PACKET
{
    public byte bmRequest;
    public byte bRequest;
    public ushort wValue;
    public ushort wIndex;
    public ushort wLength;
}

[StructLayout(LayoutKind.Sequential)]
public struct USB_STRING_DESCRIPTOR
{
    public byte bLength;
    public byte bDescriptorType;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public char[] bString;
}

/// <summary>Access flags</summary>
[Flags]
public enum FILE_ACCESS_FLAGS : uint
{
    /// <summary>Read</summary>
    GENERIC_READ = 0x80000000,

    /// <summary>Write</summary>
    GENERIC_WRITE = 0x40000000,

    /// <summary>Execute</summary>
    GENERIC_EXECUTE = 0x20000000,

    /// <summary>All</summary>
    GENERIC_ALL = 0x10000000,
}

/// <summary>Share</summary>
[Flags]
public enum FILE_SHARE : UInt32
{
    /// <summary>
    /// Enables subsequent open operations on a file or device to request read access.
    /// Otherwise, other processes cannot open the file or device if they request read access.
    /// </summary>
    READ = 0x00000001,

    /// <summary>
    /// Enables subsequent open operations on a file or device to request write access.
    /// Otherwise, other processes cannot open the file or device if they request write access.
    /// </summary>
    WRITE = 0x00000002,

    /// <summary>
    /// Enables subsequent open operations on a file or device to request delete access.
    /// Otherwise, other processes cannot open the file or device if they request delete access.
    /// If this flag is not specified, but the file or device has been opened for delete access, the function fails.
    /// </summary>
    DELETE = 0x00000004,
}

/// <summary>Disposition</summary>
public enum CreateDisposition : UInt32
{
    /// <summary>Create new</summary>
    CREATE_NEW = 1,

    /// <summary>Create always</summary>
    CREATE_ALWAYS = 2,

    /// <summary>Open exising</summary>
    OPEN_EXISTING = 3,

    /// <summary>Open always</summary>
    OPEN_ALWAYS = 4,

    /// <summary>Truncate existing</summary>
    TRUNCATE_EXISTING = 5,
}