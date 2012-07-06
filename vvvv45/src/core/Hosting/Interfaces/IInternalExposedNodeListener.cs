using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Interfaces
{
    /// <summary>
    /// IInternalExposedNodeListener is the callback interface used by IDXInternalExposedNodeService.
    /// </summary>
    [Guid("56671E96-EA42-4544-9927-D1ED86A6741A"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalExposedNodeListener
    {
        void NodeAddedCB(INode node);
        void NodeRemovedCB(INode node);
    }
}
