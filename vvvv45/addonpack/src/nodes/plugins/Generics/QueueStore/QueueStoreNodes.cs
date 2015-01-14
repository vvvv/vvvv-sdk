#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{

	#region PluginInfo
	[PluginInfo(
           Name = "QueueStore", 
           Category = "Spreads", 
           Help = "Stores a series of queues", 
           Tags = "spread,append,set,remove",
           Author = "motzi"
    )]
	#endregion PluginInfo
	public class QueueStoreNodes : QueueStore<double>
	{
		override protected ISpread<double> CloneInputSpread(ISpread<double> spread)
        {
            return spread.Clone() as ISpread<double>;
        }
	}


}
