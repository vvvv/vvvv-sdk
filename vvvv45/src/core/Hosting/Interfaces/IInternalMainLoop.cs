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
    public interface IInternalMainLoop
    {
        bool PresentUpFront
        {
            get;
        }
        
        IInternalMainLoopEvent OnPrepareGraph
        {
            get;
        }
        
        IInternalMainLoopEvent OnUpdateView
        {
            get;
        }
        
        IInternalMainLoopEvent OnRender
        {
            get;
        }
        
        IInternalMainLoopEvent OnPresent
        {
            get;
        }
        
        IInternalMainLoopEvent OnDebug
        {
            get;
        }
        
        IInternalMainLoopEvent OnNetworkSync
        {
            get;
        }
        
        IInternalMainLoopEvent OnResetCache
        {
            get;
        }

        IInternalMainLoopEvent OnInitFrame
        {
            get;
        }
    }
}
