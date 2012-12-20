using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO.Streams;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO
{
    [ComVisible(false)]
    public class IORegistry : IORegistryBase
    {
        public IORegistry()
        {
            Register(new PointerRegistry());

            Register(new PluginIORegistry());
            // Give enums a chance before using node pin
            Register(new EnumStreamRegistry());
            // Streams are more low level than spreads, so register them first
            Register(new StreamRegistry());
            Register(new PinRegistry());
            Register(new SpreadRegistry());
            
        }
    }
}
