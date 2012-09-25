using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	public class ValueStackNode : BaseStackNode<double> { }

	[PluginInfo(Name = "Stack",
		Category = "String",
		Version = "",
		Tags = "", Author = "vux"
		)]
	public class StringStackNode : BaseStackNode<string> { }

	[PluginInfo(Name = "Stack",
	Category = "Color",
	Version = "",
	Tags = "", Author = "vux"
	)]
	public class ColorStackNode : BaseStackNode<RGBAColor> { }

	[PluginInfo(Name = "Stack",
			Category = "Transform",
			Version = "",
			Tags = "", Author = "vux"
			)]
	public class TransformStackNode : BaseStackNode<Matrix4x4>
	{
	}   
}
