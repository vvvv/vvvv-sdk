using System;
using System.Runtime.InteropServices;

namespace VVVV.Hosting.Interfaces
{
    [Guid("7F2C3A3C-6600-43E5-8572-F962CFB3171F"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalMainLoopEvent
    {
        void Subscribe(IInternalMainLoopEventListener listener);
        void Unsubscribe(IInternalMainLoopEventListener listener);
    }
}
