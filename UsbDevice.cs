namespace EnumerateWinUSBDeviceGuid;
public class UsbDevice
{
    public UsbDevice()
    {
        DeviceId = String.Empty;
        Service = String.Empty;
        Name = String.Empty;
        DeviceInterfaceGUIDs = new String[0];
    }

    /// <summary>
    /// Vendor ID of the USB device, or 0 if hub/invalid DeviceID format
    /// </summary>
    public UInt16 Vid { get; internal set; }
    /// <summary>
    /// Product ID of the USB device, or 0 if hub/invalid DeviceID format
    /// </summary>
    public UInt16 Pid { get; internal set; }

    /// <summary>
    /// Device ID, e.g. USB\VID_xxxx&PID_xxxx&MI_00\6&3&0000
    /// </summary>
    public String DeviceId { get; internal set; }

    /// <summary>
    /// Windows service handling the device, in lowercase, e.g. winusb, usbser, usbaudio
    /// </summary>
    public String Service { get; internal set; }

    /// <summary>
    /// Friendly name of the device, might be blank
    /// </summary>
    public String Name { get; internal set; }

    /// <summary>
    /// List of device interface GUIDs, only filled in for winusb devices, otherwise empty array
    /// </summary>
    public String[] DeviceInterfaceGUIDs { get; internal set; }
}
