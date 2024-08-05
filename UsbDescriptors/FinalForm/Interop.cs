using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UsbDescriptors.FinalForm;

internal static class Interop
{
    internal const int CR_SUCCESS = 0;
    internal const int USB_STRING_DESCRIPTOR_TYPE = 3;

    private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
    private const uint FILE_DEVICE_USB = FILE_DEVICE_UNKNOWN;
    private const uint METHOD_BUFFERED = 0;
    private const uint FILE_ANY_ACCESS = 0;

    private const uint USB_GET_NODE_CONNECTION_INFORMATION_EX = 274;

    internal static readonly uint IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX =
        GetCtlCode(FILE_DEVICE_USB, USB_GET_NODE_CONNECTION_INFORMATION_EX, METHOD_BUFFERED, FILE_ANY_ACCESS);

    private const uint USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 260;

    internal static readonly uint IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION =
        GetCtlCode(FILE_DEVICE_USB, USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, METHOD_BUFFERED, FILE_ANY_ACCESS);

    private static uint GetCtlCode(uint deviceType, uint function, uint method, uint access) =>
        ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);
    
    [DllImport("setupapi.dll", SetLastError = true)]
    internal static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);
    
    [DllImport("setupapi.dll", SetLastError = true)]
    internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll", SetLastError = true)]
    internal static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);
    
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, uint MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
    internal static extern int CM_Get_Child(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
    internal static extern int CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, int BufferLen, uint ulFlags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
    internal static extern int CM_Get_Sibling(out uint pdnDevInst, uint dnDevInst, uint ulFlags);
    
    [DllImport("cfgmgr32.dll", SetLastError = true)]
    internal static extern uint CM_Locate_DevNodeA(out uint pdnDevInst, string pDeviceID, uint ulFlags);
    
    [DllImport("cfgmgr32.dll", SetLastError = true)]
    internal static extern uint CM_Get_DevNode_Registry_PropertyA(uint dnDevInst, uint ulProperty, IntPtr pulRegDataType, ref uint Buffer, ref uint pulLength, uint ulFlags);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool CloseHandle(IntPtr hObject);
}


[StructLayout(LayoutKind.Sequential)]
internal struct SP_DEVINFO_DATA
{
    internal readonly int cbSize;
    internal Guid ClassGuid;
    internal uint DevInst;
    internal IntPtr Reserved;

    public SP_DEVINFO_DATA()
    {
        cbSize = Marshal.SizeOf(this);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct SP_DEVICE_INTERFACE_DATA
{
    internal readonly int cbSize;
    internal Guid InterfaceClassGuid;
    internal int Flags;
    internal IntPtr Reserved;

    public SP_DEVICE_INTERFACE_DATA()
    {
        cbSize = Marshal.SizeOf(this);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct SP_DEVICE_INTERFACE_DETAIL_DATA_UNMANAGED
{
    internal readonly int cbSize;
    internal IntPtr DevicePath; // Pointer to the device path string

    public SP_DEVICE_INTERFACE_DETAIL_DATA_UNMANAGED()
    {
        cbSize = IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize; // Adjust for 64-bit or 32-bit
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_DEVICE_PROPS
{
    internal uint vid;
    internal uint pid;
    internal uint speed;
    internal uint lower_speed;
    internal uint port;
    internal bool is_USB;
    internal bool is_SCSI;
    internal bool is_CARD;
    internal bool is_UASP;
    internal bool is_VHD;
    internal bool is_Removable;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_NODE_CONNECTION_INFORMATION_EX
{
    internal uint ConnectionIndex;
    internal USB_DEVICE_DESCRIPTOR DeviceDescriptor;
    internal byte CurrentConfigurationValue;
    internal byte Speed;
    internal byte DeviceIsHub;
    internal ushort DeviceAddress;
    internal uint NumberOfOpenPipes;
    internal USB_PIPE_INFO[] PipeList;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_DEVICE_DESCRIPTOR
{
    internal byte bLength;
    internal byte bDescriptorType;
    internal ushort bcdUSB;
    internal byte bDeviceClass;
    internal byte bDeviceSubClass;
    internal byte bDeviceProtocol;
    internal byte bMaxPacketSize0;
    internal ushort idVendor;
    internal ushort idProduct;
    internal ushort bcdDevice;
    internal byte iManufacturer;
    internal byte iProduct;
    internal byte iSerialNumber;
    internal byte bNumConfigurations;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_PIPE_INFO
{
    internal USB_ENDPOINT_DESCRIPTOR EndpointDescriptor;
    internal uint ScheduleOffset;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_ENDPOINT_DESCRIPTOR
{
    internal byte bLength;
    internal byte bDescriptorType;
    internal byte bEndpointAddress;
    internal byte bmAttributes;
    internal ushort wMaxPacketSize;
    internal byte bInterval;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_DESCRIPTOR_REQUEST_WITH_STRING
{
    internal USB_DESCRIPTOR_REQUEST DescriptorRequest;
    internal USB_STRING_DESCRIPTOR StringDescriptor;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_DESCRIPTOR_REQUEST
{
    internal uint ConnectionIndex;
    internal USB_SETUP_PACKET SetupPacket;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_SETUP_PACKET
{
    internal byte bmRequest;
    internal byte bRequest;
    internal ushort wValue;
    internal ushort wIndex;
    internal ushort wLength;
}

[StructLayout(LayoutKind.Sequential)]
internal struct USB_STRING_DESCRIPTOR
{
    internal byte bLength;
    internal byte bDescriptorType;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    internal char[] bString;
}

/// <summary>Access flags</summary>
[Flags]
internal enum FILE_ACCESS_FLAGS : uint
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
internal enum FILE_SHARE : UInt32
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
internal enum CreateDisposition : UInt32
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