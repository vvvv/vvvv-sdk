using SlimDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Generic
{
    [PluginInfo(Name = "SplitAt",
                Category = "Value",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class ValueSplitAtNode : SplitAtNode<double> { }

    [PluginInfo(Name = "SplitAt",
                Category = "2d",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class Vector2DSplitAtNode : SplitAtNode<Vector2D> { }

    [PluginInfo(Name = "SplitAt",
                Category = "3d",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class Vector3DSplitAtNode : SplitAtNode<Vector3D> { }

    [PluginInfo(Name = "SplitAt",
                Category = "4d",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class Vector4DSplitAtNode : SplitAtNode<Vector4D> { }

    [PluginInfo(Name = "SplitAt",
                Category = "String",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class StringSplitAtNode : SplitAtNode<string> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Color",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class ColorSplitAtNode : SplitAtNode<RGBAColor> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Transform",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class TransformSplitAtNode : SplitAtNode<Matrix> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Raw",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class RawSplitAtNode : SplitAtNode<Stream> { }
}
