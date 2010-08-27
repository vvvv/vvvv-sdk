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
    public class Cons<T> : IPluginEvaluate
    {
        [Input("Input", IsPinGroup = true)]
        ISpread<ISpread<T>> Input;

        [Output("Output")]
        ISpread<ISpread<T>> Output;

        public void Evaluate(int SpreadMax)
        {
            Output.SliceCount = Input.SliceCount;

            for (var i = 0; i < Input.SliceCount; i++)
            {
            	Output[i] = Input[i];
            }
        }
    }

    [PluginInfo(Name = "Cons",
                Category = "Spreads",
                Version = "Generic",
                Tags = ""
                )]
    public class ValueCons : Cons<double>
    {
    }
    
    
    [PluginInfo(Name = "Cons",
                Category = "String",
                Version = "Generic",
                Tags = ""
                )]
    public class StringCons : Cons<string>
    {
    }        
    
    [PluginInfo(Name = "Cons",
                Category = "Transform",
                Version = "",
                Tags = ""
                )]
    public class TransformCons : Cons<Matrix4x4>
    {
    }        
}