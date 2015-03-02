#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Nodes.Generic;
#endregion usings

namespace VVVV.Nodes
{
 
    [PluginInfo(Name = "Pairwise",
                Category = "Spreads",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class ValuePairwise : Pairwise<double>
    {
    }
        
    [PluginInfo(Name = "Pairwise",
                Category = "2d",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class Vector2DPairwise : Pairwise<Vector2D>
    {
    }
        
    [PluginInfo(Name = "Pairwise",
                Category = "3d",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class Vector3DPairwise : Pairwise<Vector3D>
    {
    }
        
    [PluginInfo(Name = "Pairwise",
                Category = "4d",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class Vector4DPairwise : Pairwise<Vector4D>
    {
    }
        
    [PluginInfo(Name = "Pairwise",
                Category = "Color",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class ColorPairwise : Pairwise<RGBAColor>
    {
    }        
    
    [PluginInfo(Name = "Pairwise",
                Category = "String",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class StringPairwise : Pairwise<string>
    {
    }        
    
    [PluginInfo(Name = "Pairwise",
                Category = "Transform",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class TransformPairwise : Pairwise<Matrix4x4>
    {
    }        
    
    [PluginInfo(Name = "Pairwise",
                Category = "Enumerations",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class EnumPairwise : Pairwise<EnumEntry>
    {
    }        
    
    
    
    
//    public class Pairwise<T> : IPluginEvaluate
//    {
//        [Input("Input")]
//        ISpread<T> Input;
//
//        [Output("Output")]
//        ISpread<ISpread<T>> Output;
//
//        public void Evaluate(int SpreadMax)
//        {
//            var pairs = SeqModule.Pairwise(Input).ToArray();
//
//            Output.SliceCount = pairs.Length;
//
//            for (var i = 0; i < pairs.Length; i++)
//            {
//                Output[i].SliceCount = 2;
//                var pair = pairs[i];
//                Output[i][0] = pair.Item1;
//                Output[i][1] = pair.Item2;
//            }
//        }
//    }
//
//    [PluginInfo(Name = "Pairwise",
//                Category = "Spreads",
//                Version = "",
//                Tags = ""
//                )]
//    public class PairwiseValue : Pairwise<double>
//    {
//    }
//    
//    [PluginInfo(Name = "Pairwise",
//                Category = "3D",
//                Version = "",
//                Tags = ""
//                )]
//    public class PairwiseVector3D : Pairwise<Vector3D>
//    {
//    }
    
    
    
}