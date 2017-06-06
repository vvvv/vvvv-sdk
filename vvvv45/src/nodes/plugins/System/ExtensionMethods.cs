using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Windows.Forms;
using SharpDX.RawInput;
using Microsoft.Win32;
using VVVV.Utils.Win32;

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
            if (indexOfHash >= 4)
                return deviceName.Substring(4, indexOfHash - 4);
            return string.Empty;
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

        // Thanks to http://molecularmusings.wordpress.com/2011/09/05/properly-handling-keyboard-input/
        public static KeyboardInputEventArgs GetCorrectedKeyboardInputEventArgs(this KeyboardInputEventArgs args)
        {
            var virtualKey = args.Key;
            var scanCode = args.MakeCode;
            var flags = args.ScanCodeFlags;
            if ((int)virtualKey == 255)
            {
                // discard "fake keys" which are part of an escaped sequence
                return null;
            }
            //else if (virtualKey == Keys.ShiftKey)
            //{
            //    // correct left-hand / right-hand SHIFT
            //    virtualKey = (Keys)User32.MapVirtualKey((uint)scanCode, Const.MAPVK_VSC_TO_VK_EX);
            //}
            else if (virtualKey == Keys.NumLock)
            {
                // correct PAUSE/BREAK and NUM LOCK silliness, and set the extended bit
                scanCode = User32.MapVirtualKey((uint)virtualKey, Const.MAPVK_VSC_TO_VK_EX) | 0x100;
            }

            // e0 and e1 are escape sequences used for certain special keys, such as PRINT and PAUSE/BREAK.
            // see http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
            var isE0 = (flags & ScanCodeFlags.E0) != 0;
            var isE1 = (flags & ScanCodeFlags.E1) != 0;

            if (isE1)
            {
                // for escaped sequences, turn the virtual key into the correct scan code using MapVirtualKey.
                // however, MapVirtualKey is unable to map VK_PAUSE (this is a known bug), hence we map that by hand.
                if (virtualKey == Keys.Pause)
                    scanCode = 0x45;
                else
                    scanCode = User32.MapVirtualKey((uint)virtualKey, Const.MAPVK_VK_TO_VSC);
            }

            //switch (virtualKey)
            //{
              //// right-hand CONTROL and ALT have their e0 bit set
              //case Keys.ControlKey:
              //  if (isE0)
              //    virtualKey = Keys.RControlKey;
              //  else
              //    virtualKey = Keys.LControlKey;
              //  break;
 
              //case Keys.Menu:
              //  if (isE0)
              //    virtualKey = Keys.RMenu;
              //  else
              //    virtualKey = Keys.LMenu;
              //  break;
 
              //// NUMPAD ENTER has its e0 bit set
              //case Keys.Enter:
              //  if (isE0)
              //    virtualKey = Keys.Enter;
              //  break;
 
              //// the standard INSERT, DELETE, HOME, END, PRIOR and NEXT keys will always have their e0 bit set, but the
              //// corresponding keys on the NUMPAD will not.
              //case Keys.Insert:
              //  if (!isE0)
              //      virtualKey = Keys.NumPad0;
              //  break;
 
              //case Keys.Delete:
              //  if (!isE0)
              //    virtualKey = Keys.Decimal;
              //  break;
 
              //case Keys.Home:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad7;
              //  break;
 
              //case Keys.End:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad1;
              //  break;
 
              //case Keys.Prior:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad9;
              //  break;
 
              //case Keys.Next:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad3;
              //  break;
 
              //// the standard arrow keys will always have their e0 bit set, but the
              //// corresponding keys on the NUMPAD will not.
              //case Keys.Left:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad4;
              //  break;
 
              //case Keys.Right:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad6;
              //  break;
 
              //case Keys.Up:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad8;
              //  break;
 
              //case Keys.Down:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad2;
              //  break;
 
              //// NUMPAD 5 doesn't have its e0 bit set
              //case Keys.Clear:
              //  if (!isE0)
              //    virtualKey = Keys.NumPad5;
              //  break;
            //}
            return new KeyboardInputEventArgs()
            {
                Device = args.Device,
                ExtraInformation = args.ExtraInformation,
                Key = virtualKey,
                MakeCode = scanCode,
                ScanCodeFlags = flags,
                State = args.State
            };
        }
    }
}
