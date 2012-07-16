using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Interfaces
{
    /// <summary>
    /// Provides access to nodes that have been exposed for being remotely controlled
    /// </summary>
    [Guid("B032ADF8-DAD9-4046-A8FE-B4D2907C917D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalExposedNodeService
    {
        INode[] Nodes
        {
            get;
        }
        
        void Subscribe(IInternalExposedNodeListener listener);
        void Unsubscribe(IInternalExposedNodeListener listener);
    }
}
