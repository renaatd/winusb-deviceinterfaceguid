namespace EnumerateWinUSBDeviceGuid
{
    class Program
    {
        static void showHelp()
        {
            System.Console.WriteLine("Syntax: EnumerateWinUSBDeviceGuid [--all] [--winusb]\n\n" +
                "-a, --all     show all known devices (default: only present)\n" +
                "-w, --winusb  show only WinUSB devices (default: all USB devices)"
            );
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        static void Main(string[] args)
        {
            bool onlyPresentFlag = true;
            bool onlyWinUSB = false;

            foreach (var argument in args)
            {
                switch (argument.ToLowerInvariant())
                {
                    case "-a":
                    case "--all":
                        onlyPresentFlag = false;
                        break;
                    case "-w":
                    case "--winusb":
                        onlyWinUSB = true;
                        break;
                    default:
                        showHelp();
                        System.Environment.Exit(1);
                        break;
                }
            }

            var devices = EnumerateUsb.Enumerate(onlyPresentFlag);
            if (onlyWinUSB)
            {
                devices = devices.Where(x => x.Service == "winusb").ToList();
            }
            foreach (var device in devices.OrderBy(x => x.Vid).ThenBy(x => x.Pid).ThenBy(x => x.DeviceId))
            {
                Console.WriteLine("VID: 0x{0:x4}\nPID: 0x{1:x4}\nService: {2}\nDeviceID: {3}", device.Vid, device.Pid, device.Service, device.DeviceId);
                if (!String.IsNullOrEmpty(device.Name))
                {
                    Console.WriteLine("Name: {0}", device.Name);
                }
                foreach (var guid in device.DeviceInterfaceGUIDs)
                {
                    Console.WriteLine("DeviceInterfaceGUID: {0}", guid);
                }
                Console.WriteLine();
            }
        }
    }
}
