using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Interfaces
{
    /// <summary>
    /// IInternalExposedNodeListener is the callback interface used by IDXInternalExposedNodeService.
    /// </summary>
    [Guid("98B82B7E-487B-476A-A174-37FA07C1D408"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalExposedNodeListener
    {
        void NodeAddedCB(INode node);
        void NodeRemovedCB(INode node);
    }
}
