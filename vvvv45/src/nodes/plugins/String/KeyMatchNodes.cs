#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;

using VVVV.Core.Logging;
using System.Diagnostics;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "KeyMatch", 
	            Category = "String",
	            Help = "Detects pressed keys when connected with a Keyboard Node. Use the inspector to specify the keys to check.",
	            AutoEvaluate = true,
				Tags = "")]
	#endregion PluginInfo
    public class KeyMatchNode : VVVV.Nodes.Input.KeyMatchNode
	{
	}
	
	#region PluginInfo
	[PluginInfo(Name = "RadioKeyMatch", 
	            Category = "String",
	            Help = "Similiar to KeyMatch, but does not create a output pin for each key to check, but returns the index of the pressed key on its output pin.",
	            AutoEvaluate = true,
				Tags = "")]
	#endregion PluginInfo
	public class RadioKeyMatchNode: VVVV.Nodes.Input.RadioKeyMatchNode
	{
	}
}
