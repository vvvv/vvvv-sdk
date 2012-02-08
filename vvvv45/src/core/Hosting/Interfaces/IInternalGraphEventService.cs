using System;
using System.Runtime.InteropServices;

namespace VVVV.Hosting.Interfaces
{
    /// <summary>
    /// Events occur in the order as defined here.
    /// If PresentUpFront is true, OnPresent will be the first event.
    /// If this is a boygroup client, OnNetworkSync will be the last event.
    /// </summary>
    [Guid("DC614E85-007C-46E1-98F3-15D6612601D5"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalGraphEventService
    {
        bool PresentUpFront
        {
            get;
        }
        
        IInternalGraphEvent OnPrepareGraph
        {
            get;
        }
        
        IInternalGraphEvent OnUpdateView
        {
            get;
        }
        
        IInternalGraphEvent OnRender
        {
            get;
        }
        
        IInternalGraphEvent OnPresent
        {
            get;
        }
        
        IInternalGraphEvent OnDebug
        {
            get;
        }
        
        IInternalGraphEvent OnNetworkSync
        {
            get;
        }
        
        IInternalGraphEvent OnResetCache
        {
            get;
        }
    }
}
