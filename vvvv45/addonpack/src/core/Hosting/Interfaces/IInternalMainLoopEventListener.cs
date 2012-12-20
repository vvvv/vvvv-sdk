using System;
using System.Runtime.InteropServices;

namespace VVVV.Hosting.Interfaces
{
    [Guid("3A6CF89A-9BF5-4D29-A747-580AEB6259E6"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalMainLoopEventListener
    {
        void HandleEvent();
    }
}
