using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VL.Core;
using VL.Core.Properties;
using VL.Core.Viewer;
using VL.HDE.Forms;
using VL.HDE.SymbolBrowsers;
using VL.HDE.PatchEditor;
using VL.Lang.Platforms.CIL;
using VVVV.PluginInterfaces.V2;
using NuGet;
using System.Linq;
using VL.Model;
using VL.Lang.Symbols;
using System.Collections.Generic;
using Microsoft.Cci;
using System.Threading;
using Microsoft.Threading;

namespace VVVV.VL.Hosting
{
    public partial class Host : IDisposable, IQueryDelete, ICustomQueryInterface, IVLHost
    {
        public readonly VLSession Session;
        
        public Host()
        {
            // Need to set before static ctor of Settings class is called
            SettingsHACK.CustomSettingsFile = CommandLineArguments.Instance.SettingsFile;
            //setup session
            Session = new VLSession(SynchronizationContext.Current);
            // Use our own implementation
            var platform = Session.TargetPlatform as Platform;
            platform.RuntimeHost.Dispose();
            platform.RuntimeHost = new RuntimeHost(platform);
        }

        public Host Initialize(IHDEHost hdeHost, Core.Logging.ILogger logger)
        {
            RuntimeHost.Initialize(this, hdeHost, logger);
            return this;
        }

        public void Dispose()
        {
            if (FPatchEditor != null)
                FPatchEditor.Dispose();
            RuntimeHost.Dispose();
        }

        public IPlatform Platform => Session.TargetPlatform;
        public RuntimeHost RuntimeHost => (RuntimeHost)Platform.RuntimeHost;
        public HostEnvironment CciHost => Platform.Host;

        public EditorControl PatchEditor
        {
            get
            {
                if (FPatchEditor == null)
                {
                    // TODO: Please find an easier way to setup all the dependencies
                    var formManager = new FormManager(Session);
                    FPatchEditor = new EditorControl(Session, formManager, false);
                    FPatchEditor.Disposed += patchEditor_Disposed;

                    formManager.PatchEditor = this.FPatchEditor;
                }
                return FPatchEditor;
            }
        }
        EditorControl FPatchEditor;

        public IAssemblyReference UtilsAssembly
        {
            get
            {
                if (this.utilsAssembly == null)
                {
                    var assemblyIdentity = UnitHelper.GetAssemblyIdentity(typeof(VVVV.Utils.VMath.VMath).Assembly.GetName(), CciHost);
                    this.utilsAssembly = new Microsoft.Cci.Immutable.AssemblyReference(CciHost, assemblyIdentity);
                }
                return utilsAssembly;
            }
        }
        IAssemblyReference utilsAssembly;

        public INamedTypeReference InStreamType
        {
            get
            {
                if (this.inStreamType == null)
                    this.inStreamType = CciHost.PlatformType.CreateReference(
                            this.UtilsAssembly,
                            1,
                            new[] { "VVVV", "Utils", "Streams", "IInStream" });
                return this.inStreamType;
            }
        }
        INamedTypeReference inStreamType;

        public INamedTypeReference MemoryPoolType
        {
            get
            {
                if (this.memoryPoolType == null)
                    this.memoryPoolType = CciHost.PlatformType.CreateReference(
                            this.UtilsAssembly,
                            1,
                            new[] { "VVVV", "Utils", "Streams", "MemoryPool" });
                return this.memoryPoolType;
            }
        }
        INamedTypeReference memoryPoolType;

        public INamedTypeReference StreamUtilsType
        {
            get
            {
                if (this.streamUtilsType == null)
                    this.streamUtilsType = CciHost.PlatformType.CreateReference(
                            this.UtilsAssembly,
                            new[] { "VVVV", "Utils", "Streams", "StreamUtils" });
                return this.streamUtilsType;
            }
        }
        INamedTypeReference streamUtilsType;

        void patchEditor_Disposed(object sender, EventArgs e)
        {
            FPatchEditor.Disposed -= patchEditor_Disposed;
 	        FPatchEditor = null;
        }

        public void OpenPatchEditor(Node node, IHDEHost hdeHost, PatchHandle patchHandle = null)
        {
            PatchEditor.OpenCanvasOfNode(node, patchHandle);
			//setting the control also brings the window to front
            hdeHost.FiftyEditor = this;
            hdeHost.ShowVLEditor();
        }

        public void HideTooltip()
        {
            if (FPatchEditor != null)
                FPatchEditor.Tooltip.Recreate();
        }

        #region IQueryDelete implementation
        public bool DeleteMe()
        {
            return PatchEditor.QueryClose();
        }
        #endregion    
        
        //simply implementing IWin32Window here wasn't enough, so redirecting it to PatchEditor
        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
        {
            if (iid.Equals(Guid.Parse("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C")))
            {
                ppv = Marshal.GetComInterfaceForObject(PatchEditor, typeof(System.Windows.Forms.IWin32Window));
                return CustomQueryInterfaceResult.Handled;
            }
            
            ppv = IntPtr.Zero;
            return CustomQueryInterfaceResult.NotHandled;
        }

        public void CloseActiveTab(out bool windowIsGone)
        {
            windowIsGone = true;
            PatchEditor?.CloseActiveTab(out windowIsGone);
        }

        public bool OpenDocument(string filename)
        {
            var provider = PatchEditor?.NavigationMenu?.Provider;
            if (provider != null)
            {
                var document = AsyncPump.Run(() => Session.GetOrAddDocumentWithSplashScreen(filename, createNew: false));
                PatchEditor.ShowDocument(document);
                return document != null;
            }
            return false;
        }
    }
}
