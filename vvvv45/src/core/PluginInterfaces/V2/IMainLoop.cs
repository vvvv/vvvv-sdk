using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// Events occur in the order as defined here.
    /// If PresentUpFront is true, OnPresent will be the first event.
    /// If this is a boygroup client, OnNetworkSync will be the last event.
    /// </summary>
    [ComVisible(false)]
    public interface IMainLoop
    {
        bool PresentUpFront
        {
            get;
        }
        
        event EventHandler OnPrepareGraph;
        
        event EventHandler OnUpdateView;
        
        event EventHandler OnRender;
        
        event EventHandler OnPresent;
        
        event EventHandler OnDebug;
        
        event EventHandler OnNetworkSync;
        
        event EventHandler OnResetCache;
    }
}
