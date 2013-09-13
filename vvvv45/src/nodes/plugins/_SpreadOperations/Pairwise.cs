#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.FSharp.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes
{
    public class Pairwise<T> : IPluginEvaluate
    {
        [Input("Input")]
        protected ISpread<ISpread<T>> Input;

        [Output("Output 1")]
        protected ISpread<ISpread<T>> Output1;

		[Output("Output 2")]
        protected ISpread<ISpread<T>> Output2;
        
        public void Evaluate(int SpreadMax)
        {
            Output1.SliceCount = Input.SliceCount;
            Output2.SliceCount = Input.SliceCount;

            for (var spread = 0; spread<Input.SliceCount; spread++)
        	{
        		var pairs = SeqModule.Pairwise(Input[spread]).ToArray();
	
        		Output1[spread].SliceCount = pairs.Length;
	            Output2[spread].SliceCount = pairs.Length;
	
	            for (var i = 0; i < pairs.Length; i++)
	            {
	                var pair = pairs[i];
	                Output1[spread][i] = pair.Item1;
	                Output2[spread][i] = pair.Item2;
	            }
        	}
        }
    }

    
    
    
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
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class Vector2DPairwise : Pairwise<Vector2D>
    {
    }
        
    [PluginInfo(Name = "Pairwise",
                Category = "3d",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class Vector3DPairwise : Pairwise<Vector3D>
    {
    }
        
    [PluginInfo(Name = "Pairwise",
                Category = "4d",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class Vector4DPairwise : Pairwise<Vector4D>
    {
    }
        
    [PluginInfo(Name = "Pairwise",
                Category = "Color",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class ColorPairwise : Pairwise<RGBAColor>
    {
    }        
    
    [PluginInfo(Name = "Pairwise",
                Category = "String",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class StringPairwise : Pairwise<string>
    {
    }        
    
    [PluginInfo(Name = "Pairwise",
                Category = "Transform",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
                Tags = ""
                )]
    public class TransformPairwise : Pairwise<Matrix4x4>
    {
    }        
    
    [PluginInfo(Name = "Pairwise",
                Category = "Enumerations",
                Help = "Returns all combinations of successive slices. From an input ABCD returns AB, BC, CD",
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