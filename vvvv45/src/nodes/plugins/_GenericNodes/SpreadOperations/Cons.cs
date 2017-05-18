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
                Category = "2d",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class Vector2Cons : Cons<Vector2D>
    {
    }
    
    [PluginInfo(Name = "Cons",
                Category = "3d",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class Vector3Cons : Cons<Vector3D>
    {
    }
    
    [PluginInfo(Name = "Cons",
                Category = "4d",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class Vector4Cons : Cons<Vector4D>
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
        string FLastSubType;

        protected override bool Prepare()
        {
            var outputPin = FOutputContainer.GetPluginIO() as IPin;
            var subType = outputPin.GetDownstreamSubType();
            if (subType != FLastSubType)
            {
                FLastSubType = subType;
                (outputPin as IEnumOut).SetSubType(subType);
                foreach (var inputPin in FInputContainer.GetPluginIOs().OfType<IEnumIn>())
                    inputPin.SetSubType(subType);
                return true;
            }
            return base.Prepare();
        }
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