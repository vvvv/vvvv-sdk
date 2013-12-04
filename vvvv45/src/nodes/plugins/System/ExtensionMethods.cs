using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Windows.Forms;
using SharpDX.RawInput;
using Microsoft.Win32;

namespace VVVV.Nodes.Input
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Generates an edge (true, false) in the output sequence for each
        /// element received from the source sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <returns>
        /// An observable sequence containing an edge (true, false) for each 
        /// element from the source sequence.
        /// </returns>
        public static IObservable<bool> Edge<T>(this IObservable<T> source)
        {
            return source.SelectMany(_ => new[] { true, false });
        }

        public static List<Keys> ToKeyCodes(this string value)
        {
            return value.Split(',')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Select(s =>
                    {
                        Keys keyCode;
                        if (Enum.TryParse<Keys>(s, true, out keyCode))
                            return keyCode;
                        else
                            return Keys.None;
                    }
                )
                .Where(keyCode => keyCode != Keys.None)
                .ToList();
        }

        public static string GetClassCode(this DeviceInfo deviceInfo)
        {
            var deviceName = deviceInfo.DeviceName;
            var indexOfHash = deviceName.IndexOf('#');
            return deviceName.Substring(4, indexOfHash - 4);
        }

        public static string GetDeviceDescription(this DeviceInfo deviceInfo)
        {
            // remove the \??\
            var deviceName = deviceInfo.DeviceName;
            deviceName = deviceName.Substring(4);

            var split = deviceName.Split('#');

            var id_01 = split[0];    // ACPI (Class code)
            var id_02 = split[1];    // PNP0303 (SubClass code)
            var id_03 = split[2];    // 3&13c0b0c5&0 (Protocol code)
            var localMachineKey = Registry.LocalMachine;
            using (var key = localMachineKey.OpenSubKey(string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", id_01, id_02, id_03)))
                return (string)key.GetValue("DeviceDesc");
        }
    }
}
