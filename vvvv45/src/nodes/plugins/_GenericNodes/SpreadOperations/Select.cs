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
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted.",
	            Tags = "repeat, resample, duplicate, spreadop")]
    public class ValueSelectNode : Select<double> {}
}
