using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public interface IStartable
    {
        void Start();
    }
}