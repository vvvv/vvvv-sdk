using System.IO;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Raw
{
    [PluginInfo(Name = "=", Category = "Raw")]
    public class EqualsNode : Equals<Stream>
    {
        public EqualsNode()
            : base(StreamEqualityComparer.Instance)
        {
        }
    }
}
