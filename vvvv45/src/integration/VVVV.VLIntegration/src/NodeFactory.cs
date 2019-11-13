extern alias Codeplex;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Codeplex::System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VL.Core;
using VL.Lang;
using VL.Model;
using VVVV.Core.Logging;
using VVVV.Hosting;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Linq;
using VVVV.VL.Hosting;
using VVVV.VL.Hosting.IO.Streams;
using Symbols = VL.Lang.Symbols;
using Microsoft.Threading;
using VVVV.PluginInterfaces.V2.Graph;
using VL.UI.Core;
using VVVV.NuGetAssemblyLoader;
using VL.Lang.Platforms.CIL;
using System.Runtime.CompilerServices;

namespace VVVV.VL.Factories
{
    [Export(typeof(IAddonFactory))]
    public partial class NodeFactory : AbstractFileFactory<IInternalPluginHost>, IPartImportsSatisfiedNotification
    {
        private SynchronizationContext FSyncContext;
        private Solution FSyncedSolution;

        [Import]
        protected IIORegistry FIORegistry;

        public NodeFactory()
            : base(".vl")
        {
            var host = Host = new Host();
            FSyncedSolution = host.Session.CurrentSolution;
            host.Session.SolutionUpdated += HandleSolutionUpdated;
        }

        public void OnImportsSatisfied()
        {
            // Save the synchronization context of the main thread
            FSyncContext = SynchronizationContext.Current;
            // Runtime host needs to be created on the main thread due to COM access
            FHDEHost.MouseDown += HandleMouseDown;
            FHDEHost.WindowSelectionChanged += HandleWindowSelectionChanged;

            Host.Initialize(FHDEHost, FLogger);

            // Add our IO factory to the registry
            var registry = new StreamRegistry();
            FIORegistry.Register(registry, true);

            Host.Platform.CompilationUpdated += Platform_CompilationUpdated;
        }

        public override void Dispose()
        {
            FHDEHost.WindowSelectionChanged -= HandleWindowSelectionChanged;
            FHDEHost.MouseDown -= HandleMouseDown;
            Host.Session.SolutionUpdated -= HandleSolutionUpdated;
            Host.Platform.CompilationUpdated -= Platform_CompilationUpdated;
            Host.Dispose();
            base.Dispose();
        }

        private async Task Platform_CompilationUpdated(object sender, TargetCompilationUpdateEventArgs e)
        {
            await Host.RuntimeHost.UpdateAsync((CilCompilation)e.Compilation, e.Token);
        }

        private Host Host { get; }

        public override string JobStdSubPath
        {
            get { return "vl"; }
        }

        static bool SplashShown;
        internal async Task<Document> LoadDocumentAsync(string filename)
        {
            if (SplashShown)
            {
                return await VLSession.Instance.GetOrAddDocument(filename, createNew: false);
            }
            else
            {
                using (var sf = new SplashForm())
                {
                    SplashShown = true;
                    return await VLSession.Instance.GetOrAddDocumentWithSplashScreen(filename, createNew: false, splashScreen: sf);
                }
            }
        }

        public string NodeFactoryPath
        {
            get
            {
                if (FNodeFactoryPath == null)
                    FNodeFactoryPath = Path.GetDirectoryName(typeof(NodeFactory).Assembly.Location);
                return FNodeFactoryPath;
            }
        }
        string FNodeFactoryPath;

        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filePath)
        {
            var document = Host.Session.CurrentSolution.GetOrAddDocument(filePath, createNew: false, loadDependencies: false);
            foreach (var nodeDefinition in GetNodeDefinitions(document))
            {
                var (name, category, version) = GetNodeInfoKeys(nodeDefinition);
                var nodeInfo = FNodeInfoFactory.CreateNodeInfo(name, category, version, filePath, beginUpdate: true);
                UpdateNodeInfo(nodeDefinition, nodeInfo);
                yield return nodeInfo;
            }
        }

        private void UpdateNodeInfo(Node nodeDefinition, INodeInfo nodeInfo)
        {
            nodeInfo.Factory = this;
            nodeInfo.Type = NodeType.VL;
            nodeInfo.Help = nodeDefinition.Summary;
            nodeInfo.Tags = string.Join(", ", nodeDefinition.Tags);
            nodeInfo.Author = nodeDefinition.Document.Authors;
            nodeInfo.Credits = nodeDefinition.Document.Credits;
            nodeInfo.CommitUpdate();
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            // In case the plugin is already set the HDE host called us with the intention that we
            // update the plugin with a new version based on the updated node info.
            // Our plugins are just wrappers around dynamically emitted code, and they will re-emit
            // that code whenever a new target compilation arrives from the backend. We can therefor
            // ignore this call here.
            if (pluginHost.Plugin != null)
                return true;
            // The call below to Host.CreateNode can lead to a session compiled event, which in turn might
            // lead to node info updates and a HDE host calling us back here.
            // Again we don't want this behavior. So keep stupid hack for now.
            // HACK: Clean this up. The HDE host should leave the updating of nodes to the factories.
            if (this.isInCreateNode)
                return true;
            this.isInCreateNode = true;

            Host.Platform.CompilationUpdated -= Platform_CompilationUpdated;
            try
            {
                return AsyncPump.Run(async () =>
                {
                    var document = await LoadDocumentAsync(nodeInfo.Filename);
                    if (document == null)
                        return false;

                    // Load all entry points as it is very likely more nodes of the document will get used in subsequent calls
                    var nodeDefinition = default(Node);
                    var platform = Host.Platform;
                    var entryPoints = platform.LatestCompilation.EntryPointIds.ToBuilder();
                    var changed = false;
                    foreach (var n in GetNodeDefinitions(document))
                    {
                        changed |= entryPoints.Add(n.Identity);

                        var (name, category, version) = GetNodeInfoKeys(n);
                        if (name == nodeInfo.Name && category == nodeInfo.Category && version == nodeInfo.Version)
                            nodeDefinition = n;
                    }

                    if (changed)
                    {
                        // Fetch the latest compilation (previous calls had side-effects)
                        var compilation = await platform.UpdateCompilation(
                                CancellationToken.None,
                                platform.LatestCompilation.WithEntryPoints(entryPoints));
                        await Host.RuntimeHost.UpdateAsync(compilation, CancellationToken.None);
                    }

                    if (nodeDefinition != null)
                    {
                        pluginHost.Plugin = Host.RuntimeHost.CreateInstance(nodeDefinition, pluginHost, FIORegistry);
                    }

                    return true;
                });
            }
            finally
            {
                Host.Platform.CompilationUpdated += Platform_CompilationUpdated;
                this.isInCreateNode = false;
            }
        }
        bool isInCreateNode;

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            var node = nodeHost.Plugin as NodePlugin;
            if (node != null)
            {
                Host.RuntimeHost.DeleteInstance(node);
                return true;
            }
            return false;
        }

        protected override bool CloneNode(INodeInfo nodeInfo, string directory, string name, string category, string version, out string path)
        {
            var nodeDefinition = GetNodeDefinition(nodeInfo);
            if (nodeDefinition != null)
            {
                // Foo (Bar) -> VVVV.Bar.vl
                var newPrefixedCategory = Symbols.Category.VVVV.Name + "." + category;
                path = Path.Combine(directory, newPrefixedCategory + "." + name + ".vl");
                var i = 1;
                while (File.Exists(path))
                    path = Path.Combine(directory, newPrefixedCategory + "." + name + i++ + ".vl");

                if (!string.IsNullOrEmpty(version.Trim()))
                    name += " (" + version + ")";

                //when cloning from a template
                //remove the dummy info
                var removeInfo = nodeInfo.Name.StartsWith("Template");

                Document document;
                var newCategory = Symbols.Category.GetCategoryForFullName(newPrefixedCategory);
                var newDefinition = nodeDefinition
                    .WithName((NameAndVersion)name, updateReferences: false)
                    .WithIsGeneric(false);

                if (removeInfo)
                    newDefinition = newDefinition.WithTags(ImmutableArray<string>.Empty).WithSummary(null).WithRemarks(null) as Node;

                if (nodeDefinition.IsProcessDefinition)
                    document = newDefinition.Document;
                else
                {
                    //apply the new category also the parent-group of this operation
                    document = newDefinition.ParentCanvas.WithCategory(newCategory).Document;
                }

                // 45: Foo (Bar) -> 50: Foo (ExportedNodesName.Bar)
                document = document.Canvas.WithCategory(newCategory).Document;

                if (removeInfo)
                    document = (document.WithTags(ImmutableArray<string>.Empty).WithSummary(null).WithRemarks(null)).Document
                        .WithAuthors(null).WithCredits(null).WithProjectUrl(null).WithLicenseUrl(null);
                document = document.SaveAs(path);

                return true;
            }
            return base.CloneNode(nodeInfo, directory, name, category, version, out path);
        }

        void HandleMouseDown(object sender, MouseEventArgs args)
        {
            var button = args.Button;
            if (button != Mouse_Buttons.Right) return;
            var node = args.Node;
            if (node == null) return;
            var nodeInfo = node.NodeInfo;
            if (nodeInfo == null) return;
            var factory = nodeInfo.Factory;
            if (factory == null || factory != this) return;
            var nodeDefinition = GetNodeDefinition(nodeInfo);
            if (nodeDefinition == null) return;

            var n = (IInternalPluginHost) node.InternalCOMInterf;
            var path = (n.Plugin as NodePlugin)?.Object?.Context?.Path;
            var patchHandle = path.HasValue
                ? new PatchHandle(path.Value)
                : null;
            Host.OpenPatchEditor(nodeDefinition, FHDEHost, patchHandle);
        }

        void HandleWindowSelectionChanged(object sender, WindowEventArgs args)
        {
            Host.HideTooltip();
        }

        Task HandleSolutionUpdated(object sender, SolutionUpdateEventArgs args)
        {
            if (args.Kind.HasFlag(SolutionUpdateKind.AffectSession))
            {
                SyncNodeInfos(args.Solution);
            }
            return Task.CompletedTask;
        }

        void SyncNodeInfos(Solution solution)
        {
            var oldSolution = FSyncedSolution;
            FSyncedSolution = solution;
            if (solution.ChangedSince(oldSolution))
            {
                foreach (var newDocument in solution.Documents)
                {
                    var oldDocument = oldSolution.GetDocument(newDocument.Identity);
                    if (oldDocument != null && newDocument.ChangedSince(oldDocument))
                        SyncNodeInfos(oldDocument, newDocument);
                }
                foreach (var oldDocument in oldSolution.Documents)
                {
                    var newDocument = solution.GetDocument(oldDocument.Identity);
                    if (newDocument == null)
                    {
                        var nodeInfos = FNodeInfoFactory.NodeInfos.Where(n => n.Filename == oldDocument.FilePath);
                        foreach (var nodeInfo in nodeInfos)
                            FNodeInfoFactory.DestroyNodeInfo(nodeInfo);
                    }
                }
            }
        }

        void SyncNodeInfos(Document oldDocument, Document newDocument)
        {
            var oldFilePath = oldDocument.FilePath;
            var filePath = newDocument.FilePath;
            var oldMap = GetNodeDefinitions(oldDocument).ToDictionary(d => d.Identity);
            foreach (var newDef in GetNodeDefinitions(newDocument))
            {
                var oldDef = oldMap.ValueOrDefault(newDef.Identity);
                if (oldDef == newDef)
                    continue;

                var (oldName, oldCategory, oldVersion) = GetNodeInfoKeys(oldDef ?? newDef);
                var (name, category, version) = GetNodeInfoKeys(newDef);

                var nodeInfo = FNodeInfoFactory.CreateNodeInfo(oldName, oldCategory, oldVersion, oldFilePath, beginUpdate: true);
                var oldNodeInfo = nodeInfo;

                // Check if document was moved or the definition renamed.
                if (name != oldName || category != oldCategory || version != oldVersion || filePath != oldFilePath)
                {
                    if (FNodeInfoFactory.ContainsKey(name, category, version, filePath))
                    {
                        // A node info already exists (document already existed). Switch to the existing one (otherwise we'd see double entries).
                        nodeInfo = FNodeInfoFactory.CreateNodeInfo(name, category, version, filePath, beginUpdate: false);
                    }
                    else
                    {
                        // Update the existing node info.
                        FNodeInfoFactory.UpdateNodeInfo(nodeInfo, name, category, version, filePath);
                    }

                    // Point all existing nodes to the updated node info
                    foreach (var node in FHDEHost.RootNode.AsDepthFirstEnumerable())
                    {
                        if (node.NodeInfo == oldNodeInfo)
                        {
                            var patchMessage = new PatchMessage(node.Parent.NodeInfo.Filename);
                            var nodeMessage = patchMessage.AddNode(node.ID);
                            nodeMessage.CreateMe = true;
                            nodeMessage.SystemName = nodeInfo.Systemname;
                            nodeMessage.Filename = nodeInfo.Filename;
                            FHDEHost.SendXMLSnippet(patchMessage.Filename, patchMessage.ToString(), true);
                        }
                    }
                }

                if (nodeInfo == oldNodeInfo)
                    UpdateNodeInfo(newDef, nodeInfo);
                else
                    FNodeInfoFactory.DestroyNodeInfo(oldNodeInfo);
            }

            var newMap = GetNodeDefinitions(newDocument).ToDictionary(d => d.Identity);
            foreach (var oldDef in GetNodeDefinitions(oldDocument))
            {
                if (newMap.ContainsKey(oldDef.Identity))
                    continue;

                var (name, category, version) = GetNodeInfoKeys(oldDef);
                var nodeInfo = FNodeInfoFactory.CreateNodeInfo(name, category, version, filePath, beginUpdate: false);
                if (nodeInfo.Factory == this)
                    FNodeInfoFactory.DestroyNodeInfo(nodeInfo);
            }

            if (filePath != oldFilePath && File.Exists(oldFilePath))
            {
                // The document was saved under a new name but old document still exists on disk. 
                // Scan the old document again so we have double entries in the node browser as we'd have after a restart.
                foreach (var info in LoadNodeInfos(oldFilePath))
                {
                    Touch(info);
                }
            }
        }

        Node GetNodeDefinition(INodeInfo nodeInfo)
        {
            var document = AsyncPump.Run(() => LoadDocumentAsync(nodeInfo.Filename));
            foreach (var n in GetNodeDefinitions(document))
                if (Matches(n, nodeInfo))
                    return n;
            return null;
        }

        static string VLCategoryToCategory(Symbols.Category vlCategory)
        {
            var exportedNodesCategory = Symbols.Category.VVVV;
            var categoryName = vlCategory.FullName
                .Substring(exportedNodesCategory.FullName.Length)
                .TrimStart('.');
            if (string.IsNullOrEmpty(categoryName))
                categoryName = exportedNodesCategory.FullName;

            // Special vvvv beta case always making the following categories lower case
            switch (categoryName)
            {
                case "2D":
                    return "2d";
                case "3D":
                    return "3d";
                case "4D":
                    return "4d";
                default:
                    return categoryName;
            }
        }

        static string VLVersionToVersion(string vlVersion)
        {
            return vlVersion ?? string.Empty;
        }

        static (string name, string category, string version) GetNodeInfoKeys(Node nodeDefinition)
        {
            var name = nodeDefinition.Name.NamePart;
            var category = VLCategoryToCategory(nodeDefinition.Category);
            var version = VLVersionToVersion(nodeDefinition.Name.VersionPart);
            return (name, category, version);
        }

        static IEnumerable<Node> GetNodeDefinitions(Document doc)
        {
            return doc?.AllTopLevelDefinitions.Where(n => CanBeWrapped(n)) ?? Enumerable.Empty<Node>();
        }

        static bool CanBeWrapped(Node node)
        {
            return !node.IsGeneric && !node.IsAnyTypeDefinition && (node.Category == Symbols.Category.VVVV || Symbols.Category.VVVV.Contains(node.Category));
        }

        static bool Matches(Node nodeDefinition, INodeInfo nodeInfo)
        {
            var (name, category, version) = GetNodeInfoKeys(nodeDefinition);
            return name == nodeInfo.Name && category == nodeInfo.Category && version == nodeInfo.Version;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Touch(INodeInfo nodeInfo)
        {
        }
    }
}
