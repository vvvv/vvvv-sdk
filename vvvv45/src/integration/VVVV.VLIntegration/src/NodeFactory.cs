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

namespace VVVV.VL.Factories
{
    [Export(typeof(IAddonFactory))]
    public partial class NodeFactory : AbstractFileFactory<IInternalPluginHost>, IPartImportsSatisfiedNotification
    {
        class PatchExport : IDisposable
        {
            public readonly NodeFactory NodeFactory;
            public bool IsInSync;
            readonly INodeInfoFactory FNodeInfoFactory;
            INodeInfo FNodeInfo;
            NodeId? FNodeDefinitionId;
            internal Node FCurrentNodeDefinition;

            public PatchExport(NodeFactory nodeFactory, Node nodeDefinition)
            {
                NodeFactory = nodeFactory;
                FNodeInfoFactory = nodeFactory.FNodeInfoFactory;
                SerializedId = nodeDefinition.SerializedId;
                FNodeInfo = CreateNodeInfo(nodeDefinition);
            }

            public string SerializedId { get; }

            public NodeId NodeDefinitionId => InterlockedHelper.CacheNoLock(ref FNodeDefinitionId, () =>
            {
                return AsyncPump.Run(async () =>
                {
                    var document = await VLSession.Instance.GetOrAddDocument(NodeInfo.Filename, createNew: false);
                    NodeFactory.FSyncedSolution = document.Solution;
                    return document.AllTopLevelDefinitions.FirstOrDefault(n => n.SerializedId == SerializedId);
                });
            });

            public Node CurrentNodeDefinition => InterlockedHelper.CacheNoLock(ref FCurrentNodeDefinition, () =>
            {
                var nodeId = NodeDefinitionId; // has side effect. do that first.
                return NodeFactory.FSyncedSolution.GetTopLevelDefinition(NodeDefinitionId);
            });

            public void LetGo()
            {
                FNodeDefinitionId = null;
                FCurrentNodeDefinition = null;
            }

            public void Dispose()
            {
                if (FNodeInfo != null && FNodeInfo.UserData == this)
                {
                    FNodeInfoFactory.DestroyNodeInfo(FNodeInfo);
                    FNodeInfo = null;
                }
            }

            public INodeInfo NodeInfo => FNodeInfo;

            private INodeInfo CreateNodeInfo(Node nodeDefinition)
            {
                string name, category, version;
                GetNameAndVersion(nodeDefinition, out name, out category, out version);
                var nodeInfo = FNodeInfoFactory.CreateNodeInfo(name, category, version, nodeDefinition.Document.FilePath, true);
                if (nodeInfo.UserData == null)
                {
                    // If UserData != null the node info is already claimed by someone else - so leave it alone
                    nodeInfo.UserData = this;
                    nodeInfo.Factory = NodeFactory;
                    nodeInfo.Type = NodeType.VL;
                    SyncVLInfo(nodeDefinition, nodeInfo);
                }
                nodeInfo.CommitUpdate();
                return nodeInfo;
            }

            public void Sync(Node nodeDefinition)
            {
                var nodeInfo = NodeInfo;
                string name, category, version;
                GetNameAndVersion(nodeDefinition, out name, out category, out version);
                if (nodeInfo.Name != name || nodeInfo.Category != category || nodeInfo.Version != version || nodeInfo.Filename != nodeDefinition.Document.FilePath)
                {
                    if (nodeInfo.UserData == this)
                        FNodeInfoFactory.UpdateNodeInfo(nodeInfo, name, category, version, nodeDefinition.Document.FilePath);
                    else
                        FNodeInfo = nodeInfo = CreateNodeInfo(nodeDefinition);
                    var hdeHost = NodeFactory.FHDEHost;
                    foreach (var node in hdeHost.RootNode.AsDepthFirstEnumerable())
                    {
                        if (node.NodeInfo == nodeInfo)
                        {
                            var patchMessage = new PatchMessage(node.Parent.NodeInfo.Filename);
                            var nodeMessage = patchMessage.AddNode(node.ID);
                            nodeMessage.CreateMe = true;
                            nodeMessage.SystemName = nodeInfo.Systemname;
                            nodeMessage.Filename = nodeInfo.Filename;
                            hdeHost.SendXMLSnippet(patchMessage.Filename, patchMessage.ToString(), true);
                        }
                    }
                }
                SyncVLInfo(nodeDefinition, nodeInfo);
            }

            private void GetNameAndVersion(Node nodeDefinition, out string name, out string category, out string version)
            {
                category = NodeFactory.VLCategoryToCategory(nodeDefinition.Category);
                name = nodeDefinition.Name.NamePart;
                version = NodeFactory.VLVersionToVersion(nodeDefinition.Name.VersionPart);
            }

            private void SyncVLInfo(Node nodeDefinition, INodeInfo nodeInfo)
            {
                if (nodeInfo.UserData != this)
                    return; // Not our responsibility

                var tagString = string.Join(", ", nodeDefinition.Tags);
                if (nodeInfo.Help != (nodeDefinition.Summary ?? string.Empty) ||
                    nodeInfo.Tags != (tagString ?? string.Empty) ||
                    nodeInfo.Author != (nodeDefinition.Document.Authors ?? string.Empty) ||
                    nodeInfo.Credits != (nodeDefinition.Document.Credits ?? string.Empty))
                {
                    nodeInfo.BeginUpdate();
                    nodeInfo.Help = nodeDefinition.Summary;
                    nodeInfo.Tags = tagString;
                    nodeInfo.Author = nodeDefinition.Document.Authors;
                    nodeInfo.Credits = nodeDefinition.Document.Credits;
                    nodeInfo.CommitUpdate();
                }
            }
        }

        private Dictionary<string, Dictionary<string, PatchExport>> FExports = new Dictionary<string, Dictionary<string, PatchExport>>();
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
			Host.PatchEditor.OpenHostingView += PatchEditor_OpenHostingView;

			// Add our IO factory to the registry
			var registry = new StreamRegistry();
            FIORegistry.Register(registry, true);

            SynchronizationContext.Current.Post(_ => { FHDEHost.FiftyEditor = Host; }, null);
        }

        public override void Dispose()
        {
            FHDEHost.WindowSelectionChanged -= HandleWindowSelectionChanged;
            FHDEHost.MouseDown -= HandleMouseDown;
            Host.Session.SolutionUpdated -= HandleSolutionUpdated;
            Host.Dispose();
            base.Dispose();
        }

        private Host Host { get; }

        public override string JobStdSubPath
        {
            get { return "vl"; }
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

        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            var exports = GetExports(filename);
            foreach (var export in exports)
            {
                yield return export.NodeInfo;
                export.LetGo();
            }
            // To debug issue #1971
            //if (filename.Contains("VVVV.Games.Asteroids"))
            //    DebugHelper.BreakOnFileAccess(filename);
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
            try
            {
                var export = GetExport(nodeInfo);
                if (export != null)
                {
                    // It's ok to not use the CurrentNodeDefinition -> works on NodeId internally
                    pluginHost.Plugin = Host.RuntimeHost.CreateInstance(export.CurrentNodeDefinition, pluginHost, FIORegistry);
                    return true;
                }
                return false;
            }
            finally
            {
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

        bool NameCategoryVersionValid(string name, string version, string category)
        {
            return UserInputParsing.IsValidIdentifier(name) &&
                UserInputParsing.IsValidIdentifier(version, true) &&
                UserInputParsing.IsValidCategories(category);
        }

        protected override bool CloneNode(INodeInfo nodeInfo, string directory, string name, string category, string version, out string path)
        {
            var export = GetExport(nodeInfo);
            if (export != null)
            {
                // Foo (Bar) -> VVVV.Bar.vl
                var oldPrefixedCategory = Symbols.Category.VVVV.Name + "." + nodeInfo.Category;
                var newPrefixedCategory = Symbols.Category.VVVV.Name + "." + category;
                path = Path.Combine(directory, newPrefixedCategory + "." + name + ".vl");
                var i = 1;
                while (File.Exists(path))
                    path = Path.Combine(directory, newPrefixedCategory + "." + name + i++ + ".vl");
                var session = Host.Session;

                if (!string.IsNullOrEmpty(version.Trim()))
                    name += " (" + version + ")";

                //when cloning from a template
                //remove the dummy info
                var removeInfo = nodeInfo.Name.StartsWith("Template");

                Document document;
                var newCategory = Symbols.Category.GetCategoryForFullName(newPrefixedCategory);
                var nodeDefinition = export.CurrentNodeDefinition;
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

        private void HandleMouseDown(object sender, MouseEventArgs args)
        {
            var button = args.Button;
            if (button != Mouse_Buttons.Right) return;
            var node = args.Node;
            if (node == null) return;
            var nodeInfo = node.NodeInfo;
            if (nodeInfo == null) return;
            var factory = nodeInfo.Factory;
            if (factory == null || factory != this) return;
            var export = GetExport(nodeInfo);
            if (export == null) return;

			var n = (IInternalPluginHost) node.InternalCOMInterf;
			var path = (n.Plugin as NodePlugin)?.Object?.Context?.Path;
			var patchHandle = path.HasValue
				? new PatchHandle(path.Value)
				: null;
            Host.OpenPatchEditor(export.CurrentNodeDefinition, FHDEHost, patchHandle);
        }

		private void PatchEditor_OpenHostingView(uint obj)
		{
			var app = Host.RuntimeHost.HostingAppInstances.FirstOrDefault(x => x.Object?.Context?.Path.Stack?.Peek() == obj) as NodePlugin;
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

		void HandleWindowSelectionChanged(object sender, WindowEventArgs args)
        {
            Host.HideTooltip();
        }
        
        private PatchExport GetExport(INodeInfo nodeInfo)
        {
            var export = nodeInfo.UserData as PatchExport;
            if (export == null)
            {
                // Node info from cached nodelist.xml
                export = GetExports(nodeInfo.Filename)
                    .FirstOrDefault(e => 
                        e.CurrentNodeDefinition.Name.NamePart == nodeInfo.Name && 
                        VLCategoryToCategory(e.CurrentNodeDefinition.Category) == nodeInfo.Category &&
                        VLVersionToVersion(e.CurrentNodeDefinition.Name.VersionPart) == nodeInfo.Version
                    );
                nodeInfo.UserData = export;
            }
            return export;
        }

        private IEnumerable<PatchExport> GetExports(string filename)
        {
            var session = Host.Session;
            Document document = null;
            // Add the document to the session (session will check if already added to it)
            try
            {
                // Load the document only, no need for symbols now
                document = session.CurrentSolution.GetOrAddDocument(filename, createNew: false, loadDependencies: false);
            }
            catch (Exception)
            {
            }

            if (document != null)
                return SyncExports(document);

            return Enumerable.Empty<PatchExport>();
        }

        private string VLCategoryToCategory(Symbols.Category vlCategory)
        {
            var session = Host.Session;
            var exportedNodesCategory = Symbols.Category.VVVV;
            var categoryName = vlCategory.FullName
                .Substring(exportedNodesCategory.FullName.Length)
                .TrimStart('.');
            if (string.IsNullOrEmpty(categoryName))
                categoryName = exportedNodesCategory.FullName;
            return categoryName;
        }

        private string VLVersionToVersion(string vlVersion)
        {
            return vlVersion ?? string.Empty;
        }

        Task HandleSolutionUpdated(object sender, SolutionUpdateEventArgs args)
        {
            if (args.Kind.HasFlag(SolutionUpdateKind.AffectSession))
            {
                SyncExports(args.Solution);
            }
            return Task.CompletedTask;
        }

        void SyncExports(Solution solution)
        {
            foreach (var map in FExports.Values)
                foreach (var export in map.Values)
                    export.FCurrentNodeDefinition = null; // Release reference to old model
            var oldSolution = FSyncedSolution;
            FSyncedSolution = solution;
            if (solution.ChangedSince(oldSolution))
            {
                foreach (var newDocument in solution.Documents)
                {
                    var oldDocument = oldSolution.GetDocument(newDocument.Identity);
                    if (newDocument.ChangedSince(oldDocument))
                        SyncExports(newDocument);
                }
                foreach (var oldDocument in oldSolution.Documents)
                {
                    var newDocument = solution.GetDocument(oldDocument.Identity);
                    if (newDocument == null)
                    {
                        var exports = FExports.ValueOrDefault(oldDocument.FilePath);
                        foreach (var export in exports)
                            export.Value.Dispose();
                        FExports.Remove(oldDocument.FilePath);
                    }
                }
            }
        }

        IEnumerable<PatchExport> SyncExports(Document document)
        {
            var exports = FExports.ValueOrDefault(document.FilePath);
            if (exports == null)
                FExports[document.FilePath] = exports = new Dictionary<string, PatchExport>();
            foreach (var export in exports)
                export.Value.IsInSync = false;
            var nodeDefinitions = document.AllTopLevelDefinitions.Where(CanBeWrapped);
            foreach (var nodeDefinition in nodeDefinitions.Where(n => n.SerializedId != null))
            {
                var export = exports.ValueOrDefault(nodeDefinition.SerializedId);
                if (export == null)
                    exports[nodeDefinition.SerializedId] = export = new PatchExport(this, nodeDefinition);
                export.Sync(nodeDefinition);
                export.IsInSync = true;
            }
            var invalidExports = exports.Values.Where(e => !e.IsInSync).ToArray();
            // Remove them before disposing them as Dispose call has side-effects
            foreach (var export in invalidExports)
                exports.Remove(export.SerializedId);
            foreach (var export in invalidExports)
                export.Dispose();
            return exports.Values;
        }

        private static bool CanBeWrapped(Node node) =>
            !node.IsGeneric && !node.IsAnyTypeDefinition && (node.Category == Symbols.Category.VVVV || Symbols.Category.VVVV.Contains(node.Category));
    }
}
