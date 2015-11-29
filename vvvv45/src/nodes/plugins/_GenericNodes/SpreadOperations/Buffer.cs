#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Hosting.Pins;

using VVVV.Nodes.Generic;
using System.IO;
#endregion usings

namespace VVVV.Nodes
{
	
	[PluginInfo(Name = "Buffer",
	            Category = "Spreads",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ValueBufferNode : BufferNode<double>
	{
        public ValueBufferNode() : base(Copier<double>.Immutable) { }
    }
	
	[PluginInfo(Name = "Buffer",
	            Category = "Color",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ColorBufferNode : BufferNode<RGBAColor>
	{
        public ColorBufferNode() : base(Copier<RGBAColor>.Immutable) { }
    }
	
	[PluginInfo(Name = "Buffer",
	            Category = "String",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class StringBufferNode : BufferNode<string>
	{
        public StringBufferNode() : base(Copier<string>.Immutable) { }
    }
	
	[PluginInfo(Name = "Buffer",
	            Category = "Transform",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class TransformBufferNode : BufferNode<Matrix4x4>
	{
        public TransformBufferNode() : base(Copier<Matrix4x4>.Immutable) { }
	}
	
	[PluginInfo(Name = "Buffer",
	            Category = "Enumerations",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class EnumBufferNode : BufferNode<EnumEntry>
	{
        public EnumBufferNode() : base(Copiers.EnumEntry) { }
	}
	
	[PluginInfo(Name = "Buffer",
	            Category = "Raw",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class RawBufferNode : BufferNode<System.IO.Stream>
	{
        public RawBufferNode() : base(Copiers.Raw) { }
    }
}