using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
    [Guid("2B24AC85-E543-40B3-9090-2828D26978A0"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    /// <summary>
    /// HDE host as seen by vvvv.
    /// </summary>
    public interface IInternalHDEHost
    {
        void Initialize(IVVVVHost vvvvHost, INodeBrowserHost nodeBrowserHost, IWindowSwitcherHost windowSwitcherHost, IKommunikatorHost kommunikatorHost);

        void GetHDEPlugins(out IPluginBase nodeBrowser, out IPluginBase windowSwitcher, out IPluginBase kommunikator);

        void ExtractNodeInfos(string filename, string arguments, out INodeInfo[] nodeInfos);

        bool CreateNode(INode node);

        bool DestroyNode(INode node);

        INodeInfo Clone(INodeInfo nodeInfo, string path, string Name, string Category, string Version);

        void AddSearchPath(string path);

        void Shutdown();

        void RunRefactor();

        bool OnThreadMessage(IntPtr msg);

        string GetInfoString([MarshalAs(UnmanagedType.IUnknown)] object obj);
	}
}
