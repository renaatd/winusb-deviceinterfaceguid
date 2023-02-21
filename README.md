# winusb-deviceinterfaceguid
Enumerate USB/WinUSB devices - VID, PID, and WinUSB DeviceInterfaceGuid

The Windows WinUSB API needs the DeviceInterfaceGUID to open a handle to a device. Detecting USB device insertion/removal is normally done via the WMI API using the USB VID/PID.

This example code enumerates all USB devices present, and prints VID / PID / service / DeviceInterfaceGUID (only for WinUSB devices) and a device name, if known. Optionally it can print all known USB devices on the system, or filter on only the WinUSB devices.

## Compile
```
dotnet restore
dotnet build -c Release
dotnet publish -c Release
```

## Example output
```
.\EnumerateWinUSBDeviceGuid.exe --winusb --all
VID: 0x05ac
PID: 0x12a8
Service: winusb
DeviceID: USB\VID_05AC&PID_12A8&MI_01\8&259CB5B0&2&0001
Name: Apple Mobile Device USB Device
DeviceInterfaceGUID: {664be590-54bd-4964-8a8c-6cd1314f6dc2}

VID: 0x1d50
PID: 0x606f
Service: winusb
DeviceID: USB\VID_1D50&PID_606F&MI_00\8&3233064A&0&0000
Name: candleLight USB to CAN adapter
DeviceInterfaceGUID: {c15b4308-04d3-11e6-b3ea-6057189e6443}

VID: 0x1d50
PID: 0x606f
Service: winusb
DeviceID: USB\VID_1D50&PID_606F&MI_01\8&3233064A&0&0001
Name: candleLight firmware upgrade interface
```