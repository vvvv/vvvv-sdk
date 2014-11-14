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
using VVVV.Hosting.Factories;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;

namespace VVVV.VL.Factories
{
    [Export(typeof(IAddonFactory))]
    public class NodeFactory : AbstractFileFactory<IInternalPluginHost>
    {
        private readonly ServiceProvider FServiceProvider;
        private readonly Solution FSolution;
        private readonly Serializer FSerializer;
        private VLProject FProject;

        [Import]
        protected INodeInfoFactory FNodeInfoFactory;

        public NodeFactory()
            : base(".vl")
        {
            FServiceProvider = new ServiceProvider();
            FSolution = new Solution(string.Empty, FServiceProvider);
            FSerializer = new Serializer();
            VLSerializerRegistration.Register(FSerializer);
            FServiceProvider.RegisterService(FSerializer);
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
                FProject.Documents.Add(VLDocument.FromFile(Path.Combine(projectDir, "StdLib2.vl"), FSerializer, FProject.SymbolHost));
            }
            // See if a VL document with that filename is already loaded
            var document = FProject.Documents.OfType<VLDocument>()
                .FirstOrDefault(doc => doc.LocalPath.Equals(filename, StringComparison.CurrentCultureIgnoreCase));
            if (document == null)
            {
                // Not loaded yet - add it to our one and only project file
                document = VLDocument.FromFile(filename, FSerializer, FProject.SymbolHost);
                FProject.Documents.Add(document);
            }
            // Return a node info for each type
            // TODO: Only return for types which have the "isRoot" flag set
            var types = document.Namespaces.SelectMany(n => n.Types);
            foreach (var type in types)
            {
                // TODO: Add more info to node info
                var nodeInfo = FNodeInfoFactory.CreateNodeInfo(type.Name, type.Namespace.Name, string.Empty, filename, true);
                nodeInfo.UserData = type;
                nodeInfo.CommitUpdate();
                yield return nodeInfo;
            }
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            var type = nodeInfo.UserData as VLType;
            throw new NotImplementedException();
        }

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            throw new NotImplementedException();
        }
    }
}
