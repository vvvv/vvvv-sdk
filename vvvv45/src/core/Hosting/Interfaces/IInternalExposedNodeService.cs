using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Interfaces
{
    /// <summary>
    /// Provides access to nodes that have been exposed for being remotely controlled
    /// </summary>
    [Guid("60C92F66-6EF7-4017-9D5C-8DAF508CB405"),
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
