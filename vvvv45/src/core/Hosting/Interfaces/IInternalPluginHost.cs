using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Interfaces
{
    [Guid("21230B31-1929-44F8-B8C0-03E5C2AA42EF"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternalPluginHost : IPluginHost2
    {
        IPluginBase Plugin
        {
            get;
            set;
        }

        IWin32Window Win32Window
        {
            get;
            set;
        }
        
        IPluginConnections Connections
        {
            set;
        }
        
        IPluginDXLayer DXLayer
        {
            set;
        }
        
        IPluginDXMesh DXMesh
        {
            set;
        }
        
        IPluginDXResource DXResource
        {
            set;
        }
        
        IPluginDXTexture DXTexture
        {
            set;
        }
        
        IPluginDXTexture2 DXTexture2
        {
            set;
        }
        
        void Subscribe(IInternalPluginHostListener listener);
        void Unsubscribe(IInternalPluginHostListener listener);
        
        IDXTextureOut CreateTextureOutput2(IDXTexturePin texturePin, string name, TSliceMode sliceMode, TPinVisibility visibility);
        IDXMeshOut CreateMeshOutput2(IDXMeshPin meshPin, string name, TSliceMode sliceMode, TPinVisibility visibility);
    }
}
