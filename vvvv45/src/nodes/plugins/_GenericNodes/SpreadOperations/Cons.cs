#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using SlimDX;
using VVVV.Utils.Streams;
using VVVV.Nodes.Generic;
#endregion usings

namespace VVVV.Nodes
{
    [PluginInfo(Name = "Cons",
                Category = "Spreads",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class ValueCons : Cons<double>
    {
    }
        
    [PluginInfo(Name = "Cons",
                Category = "Color",
                Help = "Concatenates all Input spreads.",
                Tags = ""
                )]
    public class ColorCons : Cons<RGBAColor>
    {
    }        
    
    [PluginInfo(Name = "Cons",
                Category = "String",
                Help = "Concatenates all Input spreads.",
                Tags = ""
                )]
    public class StringCons : Cons<string>
    {
    }        
    
    [PluginInfo(Name = "Cons",
                Category = "Transform",
                Help = "Concatenates all Input spreads.",
                Tags = ""
                )]
    public class TransformCons : Cons<Matrix>
    {
    }        
    
    [PluginInfo(Name = "Cons",
                Category = "Enumerations",
                Help = "Concatenates all Input spreads.",
                Tags = ""
                )]
    public class EnumCons : Cons<EnumEntry>
    {
    }

    [PluginInfo(Name = "Cons",
            Category = "Raw",
            Help = "Concatenates all Input spreads.",
            Tags = ""
            )]
    public class RawCons : Cons<System.IO.Stream>
    {
    }
}