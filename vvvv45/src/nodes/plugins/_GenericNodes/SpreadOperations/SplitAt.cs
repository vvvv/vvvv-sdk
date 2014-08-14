using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Generic
{
    [PluginInfo(Name = "SplitAt",
                Category = "Value",
                Help = "Splits a spread at the given index.",
                Tags = "generic, spreadop"
                )]
    public class ValueSplitAtNode : SplitAtNode<double> { }
}
