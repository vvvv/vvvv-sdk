using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;

namespace VVVV.VL.Factories
{
    [Export(typeof(IAddonFactory))]
    public class NodeFactory : AbstractFileFactory<IInternalPluginHost>
    {
        public override string JobStdSubPath
        {
            get { return "vl"; }
        }

        public NodeFactory()
            : base(".vl")
        {

        }

        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            yield break;
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            throw new NotImplementedException();
        }

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            throw new NotImplementedException();
        }
    }
}
