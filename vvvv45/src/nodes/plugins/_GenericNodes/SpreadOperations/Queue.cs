#region usings
using System;
using System.IO;
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
#endregion usings

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Queue",
	            Category = "Spreads",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ValueQueueNode : QueueNode<double>
    {
        public ValueQueueNode() : base(Copier<double>.Immutable) { }
    }

    [PluginInfo(Name = "Queue",
	            Category = "Color",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ColorQueueNode : QueueNode<RGBAColor>
    {
        public ColorQueueNode() : base(Copier<RGBAColor>.Immutable) { }
    }

    [PluginInfo(Name = "Queue",
	            Category = "String",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class StringQueueNode : QueueNode<string>
    {
        public StringQueueNode() : base(Copier<string>.Immutable) { }
    }

    [PluginInfo(Name = "Queue",
	            Category = "Transform",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class TransformQueueNode : QueueNode<Matrix4x4>
    {
        public TransformQueueNode() : base(Copier<Matrix4x4>.Immutable) { }
    }

    [PluginInfo(Name = "Queue",
	            Category = "Enumerations",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class EnumQueueNode : QueueNode<EnumEntry>
    {
        public EnumQueueNode() : base(Copiers.EnumEntry) { }
    }

    [PluginInfo(Name = "Queue",
	            Category = "Raw",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class RawQueueNode : QueueNode<System.IO.Stream>
	{
        public RawQueueNode() : base(Copiers.Raw) { }
	}
}