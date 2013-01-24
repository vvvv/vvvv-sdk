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
        protected ISpread<ISpread<T>> Input;

        [Output("Output")]
        protected ISpread<ISpread<T>> Output;
        
        private readonly ISpread<T>[] FBuffer = new ISpread<T>[16];

        public void Evaluate(int SpreadMax)
        {
//        	var outputStream = Output.GetStream();
//        	var inputStream = Input.GetStream();
//        	
//        	outputStream.Length = inputStream.Length;

//        	while (!outputStream.Eof)
//        	{
//        		int numSlicesRead = inputStream.Read(FBuffer, 0, FBuffer.Length);
//        		outputStream.Write(FBuffer, 0, numSlicesRead);
//        	}
			Output.SliceCount = Input.SliceCount;
            for (var i = 0; i < Input.SliceCount; i++)
            	Output[i] = Input[i];
        }
    }

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
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class ColorCons : Cons<RGBAColor>
    {
    }        
    
    [PluginInfo(Name = "Cons",
                Category = "String",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class StringCons : Cons<string>
    {
    }        
    
    [PluginInfo(Name = "Cons",
                Category = "Transform",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class TransformCons : Cons<Matrix4x4>
    {
    }        
    
    [PluginInfo(Name = "Cons",
                Category = "Enumerations",
                Help = "Concatenates all input spreads to one output spread",
                Tags = ""
                )]
    public class EnumCons : Cons<EnumEntry>
    {
    }

    [PluginInfo(Name = "Cons",
            Category = "Raw",
            Help = "Concatenates all input spreads to one output spread",
            Tags = ""
            )]
    public class RawCons : Cons<System.IO.Stream>
    {
    }
}