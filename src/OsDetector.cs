using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace AwsUtility.MfaLogin
{
    public class OsDetector
    {
        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }



}
