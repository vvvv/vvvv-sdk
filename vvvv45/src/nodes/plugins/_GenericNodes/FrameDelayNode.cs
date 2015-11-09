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
        protected override RGBAColor CloneSlice(RGBAColor slice)
        {
            return slice;
        }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Raw",
                Help = "Delays the input value one calculation frame.",
                Tags = "generic")]
    public class RawFrameDelayNode : FrameDelayNode<System.IO.Stream>
    {
        private static readonly byte[] buffer = new byte[4096];

        protected override System.IO.Stream CloneSlice(System.IO.Stream slice)
        {
            var clone = new System.IO.MemoryStream((int)slice.Length);
            slice.Position = 0;
            slice.CopyTo(clone, buffer);
            return clone;
        }
    }

    [PluginInfo(Name = "FrameDelay", Category = "String",
                Help = "Delays the input string one calculation frame.",
                Tags = "generic")]
    public class StringFrameDelayNode : FrameDelayNode<string>
    {
        protected override string CloneSlice(string slice)
        {
            return slice;
        }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Value",
                Help = "Delays the input value one calculation frame.",
                Tags = "generic")]
    public class ValueFrameDelayNode : FrameDelayNode<double>
    {
        protected override double CloneSlice(double slice)
        {
            return slice;
        }
    }

    [PluginInfo(Name = "FrameDelay", Category = "Transform",
                Help = "Delays the input matrix one calculation frame.",
                Tags = "generic")]
    public class TransformFrameDelayNode : FrameDelayNode<Matrix>
    {
        protected override Matrix CloneSlice(Matrix slice)
        {
            return slice;
        }
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
