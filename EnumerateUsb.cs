using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using Vanara.InteropServices;
using Vanara.PInvoke;

namespace EnumerateWinUSBDeviceGuid;
public static class EnumerateUsb
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public unsafe static List<UsbDevice> Enumerate(bool onlyPresent)
    {
        var result = new List<UsbDevice>();

        // use GUID_DEVINTERFACE_USB_DEVICE to find all USB devices connected to a hub
        // https://learn.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-usb-device
        Guid guidGenericUsb = new Guid("{A5DCBF10-6530-11D2-901F-00C04FB951ED}");
        const string enumerator = "USB"; // using null instead -> returns *all* devices in the system
        Regex regex_vid_pid = new Regex(@"^USB\\VID_(....)&PID_(....)");

        SetupAPI.DIGCF flags = SetupAPI.DIGCF.DIGCF_ALLCLASSES;
        if (onlyPresent)
        {
            flags |= SetupAPI.DIGCF.DIGCF_PRESENT;
        }

        using (var deviceInfoSetHandle = SetupAPI.SetupDiGetClassDevs(guidGenericUsb, enumerator, IntPtr.Zero, flags))
        {
            var deviceInfoData = new SetupAPI.SP_DEVINFO_DATA();
            // With Vanara, we must explicitly set the size of the structure
            deviceInfoData.cbSize = (uint)sizeof(SetupAPI.SP_DEVINFO_DATA);
            for (uint memberIndex = 0; SetupAPI.SetupDiEnumDeviceInfo(deviceInfoSetHandle, memberIndex, ref deviceInfoData); memberIndex++)
            {
                if (CfgMgr32.CM_Get_Device_ID_Size(out var len, deviceInfoData.DevInst) != CfgMgr32.CONFIGRET.CR_SUCCESS)
                {
                    continue;
                }
                var sb = new StringBuilder((int)len + 1);
                if (CfgMgr32.CM_Get_Device_ID(deviceInfoData.DevInst, sb, len + 1) != CfgMgr32.CONFIGRET.CR_SUCCESS)
                {
                    continue;
                }
                var deviceId = sb.ToString();

                var thisDevice = new UsbDevice
                {
                    DeviceId = deviceId,
                    // Adapted from libusbp get_driver_name
                    // https://github.com/pololu/libusbp/blob/e5d6e826ec1e6164c78ebc7315d0997fdbf7b3d7/src/windows/generic_interface_windows.c
                    Service = GetDeviceRegistryProperty(deviceInfoSetHandle, ref deviceInfoData, SetupAPI.SPDRP.SPDRP_SERVICE).ToLowerInvariant(),
                    Name = GetDeviceRegistryProperty(deviceInfoSetHandle, ref deviceInfoData, SetupAPI.SPDRP.SPDRP_FRIENDLYNAME),
                };
                var regex_result = regex_vid_pid.Match(deviceId);
                if (regex_result.Success)
                {
                    thisDevice.Vid = UInt16.Parse(regex_result.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                    thisDevice.Pid = UInt16.Parse(regex_result.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                }

                // Adapted from libusbp get_first_device_interface_guid(HDEVINFO list, ...)
                using (var sh = SetupAPI.SetupDiOpenDevRegKey(deviceInfoSetHandle, deviceInfoData, SetupAPI.DICS_FLAG.DICS_FLAG_GLOBAL, 0, SetupAPI.DIREG.DIREG_DEV, RegistryRights.ReadKey))
                {
                    if (!sh.IsInvalid && !sh.IsClosed)
                    {
                        var key = Microsoft.Win32.RegistryKey.FromHandle(sh);
                        var rawValue = key.GetValue("DeviceInterfaceGUIDs");
                        var deviceInterfaceGUIDs = rawValue as string[];
                        thisDevice.DeviceInterfaceGUIDs = deviceInterfaceGUIDs ?? new string[0];
                    }
                }

                result.Add(thisDevice);
            }
        }
        return result;
    }

    /// <summary>
    /// Return a string type device registry property
    /// </summary>
    /// <returns>The device registry property, or String.Empty() if there is no such property or it is not a string</returns>
    public static string GetDeviceRegistryProperty(SetupAPI.SafeHDEVINFO handle, ref SetupAPI.SP_DEVINFO_DATA deviceInfo, SetupAPI.SPDRP property)
    {
        // first call is without buffer -> always returns false, but that's OK, requiredSize is filled in
        // Error code is either 122 (ERROR_INSUFFICIENT_BUFFER, The data area passed to a system call is too small.) or 13(ERROR_INVALID_DATA, The data is invalid.)
        bool getSizeSuccess = SetupAPI.SetupDiGetDeviceRegistryProperty(handle, deviceInfo, property, out REG_VALUE_TYPE _, IntPtr.Zero, 0, out uint requiredSize);
        if (requiredSize == 0)
        {
            return string.Empty;
        }

        // https://github.com/dahall/Vanara/wiki/Native-memory-helper-classes
        using var mem = new SafeCoTaskMemHandle(requiredSize);
        bool success = SetupAPI.SetupDiGetDeviceRegistryProperty(handle, deviceInfo, property,
            out REG_VALUE_TYPE dataType, mem, mem.Size, out requiredSize);
        if (success && dataType == REG_VALUE_TYPE.REG_SZ)
        {
            return System.Text.Encoding.Unicode.GetString(mem.GetBytes(), 0, (int)requiredSize).TrimEnd('\0');
        }
        return string.Empty;
    }
}