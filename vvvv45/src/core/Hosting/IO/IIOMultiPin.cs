using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    [ComVisible(false)]
    public interface IIOMultiPin
    {
        IIOContainer BaseContainer { get; }
        IIOContainer[] AssociatedContainers { get; }
    }
}
