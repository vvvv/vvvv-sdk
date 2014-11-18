using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Core;
using VL.Core.Model;
using VL.Core.Serialization;
using VL.Lang.Model;
using VL.Lang.Serialization;
using VL.HDE.View;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;
using VL.Core.Viewer;
using VL.Core.Menu;

namespace VVVV.VL.Factories
{
    [Export(typeof(IAddonFactory))]
    public class NodeFactory : AbstractFileFactory<IInternalPluginHost>, IPartImportsSatisfiedNotification
    {
        private readonly ServiceProvider FServiceProvider;
        private readonly Solution FSolution;
        private readonly Serializer FSerializer;
        private VLProject FProject;
        private MappingRegistry FGlobalViewRegistry;
        private EditorForm FPatchEditor;

        [Import]
        protected IIORegistry FIORegistry;

        public NodeFactory()
            : base(".vl")
        {
            FServiceProvider = new ServiceProvider();
            FSolution = new Solution(string.Empty, FServiceProvider);
            FSerializer = new Serializer();
            VLSerializerRegistration.Register(FSerializer);
            FServiceProvider.RegisterService(FSerializer);
        }

        public void OnImportsSatisfied()
        {
            FHDEHost.MouseDown += HandleMouseDown;
        }

        public override string JobStdSubPath
        {
            get { return "vl"; }
        }

        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            if (FProject == null)
            {
                // Sets up symbol host, target platform etc. takes a while
                var projectDir = Path.GetDirectoryName(typeof(VLProject).Assembly.Location);
                FProject = new VLProject(Path.Combine(projectDir, "MyVLProject.vlproj"));
                FSolution.Projects.Add(FProject);
                // Add standard lib
                FProject.AddVLDocumentFromFile(Path.Combine(projectDir, "StdLib2.vl"));
                // Add the new nodes lib
                FProject.References.BeginUpdate();
                FProject.References.Add(new AssemblyReference(Path.Combine(projectDir, "NewNodes.dll")));
                FProject.References.EndUpdate();
                // Whenever a command gets executed re-compile the project
                FProject.CommandHistory.CommandExecuted += CommandHistory_CommandExecuted;
            }
            // Add the document to the project (project will check if already added to it)
            var document = FProject.AddVLDocumentFromFile(filename);
            // Return a node info for each type
            // TODO: Only return for types which have the "isRoot" flag set
            var types = document.Namespaces.SelectMany(n => n.Types);
            foreach (var type in types)
            {
                // TODO: Add more info to node info
                var nodeInfo = FNodeInfoFactory.CreateNodeInfo(type.Name, type.Namespace.Name, string.Empty, filename, true);
                nodeInfo.UserData = type;
                nodeInfo.Factory = this;
                nodeInfo.Type = NodeType.Dynamic;
                nodeInfo.CommitUpdate();
                yield return nodeInfo;
            }
        }

        void CommandHistory_CommandExecuted(object sender, global::VL.Core.Commands.CommandExecutedEventArgs e)
        {
            FProject.Compile();
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            var type = nodeInfo.UserData as VLType;
            var node = new Node(type, FProject.RuntimeHost, nodeHost, FIORegistry);
            nodeHost.Plugin = node;
            FProject.Compile();
            return true;
        }

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            var node = nodeHost.Plugin as Node;
            node.Dispose();
            return true;
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
            var vlType = nodeInfo.UserData as VLType;
            if (vlType == null) return;
            OpenPatchEditor(vlType);
        }

        void OpenPatchEditor(VLType vlType)
        {
            if (FPatchEditor == null)
            {
                // TODO: Please find an easier way to setup all the dependencies
                FPatchEditor = new EditorForm();
                FGlobalViewRegistry = new MappingRegistry();
                FGlobalViewRegistry.RegisterDefaultInstance(FPatchEditor);
                FGlobalViewRegistry.RegisterDefaultInstance(FSolution);
                FGlobalViewRegistry.RegisterService<IViewerService, ViewerService>();
                FGlobalViewRegistry.RegisterService<ISelectionService, SelectionService>();
                FGlobalViewRegistry.RegisterService<IMenuProvider, MainMenuProvider>();
                FPatchEditor.Registry = FGlobalViewRegistry;
            }
            FPatchEditor.OpenPatch(vlType);
            FPatchEditor.Show();
        }
    }
}
