using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Stack",
		Category = "Value",
		Version = "",
		Tags = "",Author="vux"
		)]
	public class ValueStackNode : StackNode<double> { }

	[PluginInfo(Name = "Stack",
		Category = "String",
		Version = "",
		Tags = "", Author = "vux"
		)]
	public class StringStackNode : StackNode<string> { }

	[PluginInfo(Name = "Stack",
	Category = "Color",
	Version = "",
	Tags = "", Author = "vux"
	)]
	public class ColorStackNode : StackNode<RGBAColor> { }

	[PluginInfo(Name = "Stack",
			Category = "Transform",
			Version = "",
			Tags = "", Author = "vux"
			)]
	public class TransformStackNode : StackNode<Matrix4x4>
	{
	}   
}
