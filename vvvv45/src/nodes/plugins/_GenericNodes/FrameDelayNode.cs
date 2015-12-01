using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Nodes.Generic;
using VVVV.Utils.VMath;
using SlimDX;
using VVVV.Utils.Streams;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "FrameDelay", Category = "Color",
                Help = "Delays the input color one calculation frame.",
                Tags = "generic")]
    public class ColorFrameDelayNode : FrameDelayNode<RGBAColor>
    {
        public ColorFrameDelayNode() : base(Copier<RGBAColor>.Immutable) { }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Raw",
                Help = "Delays the input value one calculation frame.",
                Tags = "generic")]
    public class RawFrameDelayNode : FrameDelayNode<System.IO.Stream>
    {
        public RawFrameDelayNode() : base(Copiers.Raw) { }
    }

    [PluginInfo(Name = "FrameDelay", Category = "String",
                Help = "Delays the input string one calculation frame.",
                Tags = "generic")]
    public class StringFrameDelayNode : FrameDelayNode<string>
    {
        public StringFrameDelayNode() : base(Copier<string>.Immutable) { }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Value",
                Help = "Delays the input value one calculation frame.",
                Tags = "generic")]
    public class ValueFrameDelayNode : FrameDelayNode<double>
    {
        public ValueFrameDelayNode() : base(Copier<double>.Immutable) { }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Transform",
                Help = "Delays the input matrix one calculation frame.",
                Tags = "generic")]
    public class TransformFrameDelayNode : FrameDelayNode<Matrix>
    {
        public TransformFrameDelayNode() : base(Copier<Matrix>.Immutable) { }
    }

//    [PluginInfo(Name = "FrameDelay", Category = "Enumerations",
//                Help = "Delays the input value one calculation frame.",
//                Tags = "generic")]
//    public class EnumerationsFrameDelayNode : FrameDelayNode<EnumEntry>
//    {
//        protected override EnumEntry CloneSlice(EnumEntry slice)
//        {
//            return slice;
//        }
//    }

}
