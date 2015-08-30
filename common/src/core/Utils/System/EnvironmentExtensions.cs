using System;

namespace System
{
    public static class EnvironmentExtensions
    {
        // From http://stackoverflow.com/questions/2732432/how-to-tell-if-the-os-is-windows-xp-or-higher
        public static bool IsWinXPOrHigher(this OperatingSystem OS)
        {
            return (OS.Platform == PlatformID.Win32NT) && ((OS.Version.Major > 5) || ((OS.Version.Major == 5) && (OS.Version.Minor >= 1)));
        }

        // From http://stackoverflow.com/questions/2732432/how-to-tell-if-the-os-is-windows-xp-or-higher
        public static bool IsWinVistaOrHigher(this OperatingSystem OS)
        {
            return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
        }
    }
}
