using System;
using System.Runtime.InteropServices;
using VL.Core;
using VL.HDE.Forms;
using VL.HDE.PatchEditor;
using VL.Lang.Platforms.CIL;
using VVVV.PluginInterfaces.V2;
using VL.Model;
using VL.Lang.Symbols;
using Microsoft.Cci;
using System.Threading;
using Microsoft.Threading;
using VL.UI.Core;
using VVVV.Core.Logging;
using VVVV.NuGetAssemblyLoader;
using System.Linq;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.VL.Hosting
{
    public partial class Host : IDisposable, IQueryDelete, ICustomQueryInterface, IVLHost
    {
        public readonly VLSession Session;
        IHDEHost FHDEHost;
        
        public Host()
        {
            //setup session
            Session = new VLSession("beta", SynchronizationContext.Current);
            // Use our own implementation
            var platform = Session.TargetPlatform as Platform;
            platform.RuntimeHost.Dispose();
            platform.RuntimeHost = new RuntimeHost(platform);

        }

        public Host Initialize(IHDEHost hdeHost, ILogger logger)
        {
            FHDEHost = hdeHost;
            RuntimeHost.Initialize(this, hdeHost, logger);
            SynchronizationContext.Current.Post(_ => { FHDEHost.FiftyEditor = this; }, null);
            return this;
        }

        public void Dispose()
        {
            if (FPatchEditor != null)
            {
                FPatchEditor.OpenHostingView -= PatchEditor_OpenHostingView;
                FPatchEditor.VisibleChanged -= PatchEditor_VisibleChanged;
                FPatchEditor.Dispose();
            }
            RuntimeHost.Dispose();
        }

        public Platform Platform => (Platform)Session.TargetPlatform;
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
                    FPatchEditor = new EditorControl(Session, formManager);
                    FPatchEditor.OpenHostingView += PatchEditor_OpenHostingView;
                    FPatchEditor.VisibleChanged += PatchEditor_VisibleChanged;

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
                    var assemblyIdentity = UnitHelper.GetAssemblyIdentity(typeof(global::VVVV.Utils.VMath.VMath).Assembly.GetName(), CciHost);
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
                using (var splashScreen = new SplashForm())
                {
                    var document = AsyncPump.Run(() => Session.GetOrAddDocumentWithSplashScreen(filename, createNew: false, splashScreen: splashScreen));
                    PatchEditor.ShowDocument(document);
                    return document != null;
                }
            }
            return false;
        }

        private void PatchEditor_OpenHostingView(uint obj)
        {
            var app = RuntimeHost.HostingAppInstances.FirstOrDefault(x => x.Object?.Context?.Path.Stack?.Peek() == obj) as NodePlugin;
            if (app != null)
            {
                var node = FHDEHost.GetNodeFromPath(app.PluginHost.GetNodePath(false));
                var patch = FHDEHost.GetNodeFromPath(app.PluginHost.ParentNode.GetNodePath(false));
                if (patch != null)
                {
                    FHDEHost.ShowEditor(patch);
                    FHDEHost.SelectNodes(new INode2[1] { node });
                }
            }
        }

        private void PatchEditor_VisibleChanged(object sender, EventArgs e)
        {
            if (!extensionsLoaded)
            {
                extensionsLoaded = true;
                using (var sf = new SplashForm())
                {
                    AsyncPump.Run(() => Session.LoadExtensionDocuments(sf));
                }
            }
        }
        bool extensionsLoaded;
    }
}
