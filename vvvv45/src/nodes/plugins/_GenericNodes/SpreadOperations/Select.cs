using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.Streams;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
  
    [PluginInfo(Name = "Select",
                Category = "Value",
                Help = "Select which slices and how many form the output spread",
	            Tags = "repeat, resample")]
    public class ValueSelectNode : Select<double> {}
}
