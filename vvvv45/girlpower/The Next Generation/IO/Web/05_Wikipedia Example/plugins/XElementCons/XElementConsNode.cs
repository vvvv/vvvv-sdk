#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Xml;
using System.Xml.Linq;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Cons", Category = "XElement", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class XElementConsNode : VVVV.Nodes.Cons<XElement>
	{
	}
}
