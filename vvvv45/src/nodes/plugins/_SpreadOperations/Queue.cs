#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Hosting.Pins;
#endregion usings

namespace VVVV.Nodes
{
    public class Queue<T> : IPluginEvaluate
    {
        [Input("Input")]
        ISpread<T> Input;
        
        [Input("do Insert", SliceMode = SliceMode.Single)]
        ISpread<bool> DoInsert;
      
        [Input("Frame Count", SliceMode = SliceMode.Single, MinValue = 0, DefaultValue = 1)]
        ISpread<int> FrameCount;

        
        [Output("Output")]
        ISpread<ISpread<T>> Output;

        [Output("Frames")]
        ISpread<int> FrameSlices;
        
        List<List<T>> FBuffer = new List<List<T>>();        
        
        public void Evaluate(int SpreadMax)
        {
        	if (DoInsert[0])
        		FBuffer.Insert(0, Input.ToList());
			
        	var frameCount = FrameCount[0];        	
        	if (FBuffer.Count > frameCount)
        		FBuffer.RemoveRange(frameCount, FBuffer.Count-frameCount);
        	        	
        	Output.SliceCount = FBuffer.Count;
        	FrameSlices.SliceCount = FBuffer.Count+1;
        	
        	var k = 0;
        	FrameSlices[0] = 0;
            for (var i = 0; i < FBuffer.Count; i++)
            {
				Output[i] = FBuffer[i].ToSpread();
            	k += FBuffer[i].Count;
	        	FrameSlices[i+1] = k;
            }
        }
    }

    
    [PluginInfo(Name = "Queue",
                Category = "Spreads",
                Version = "",
                Tags = ""
                )]
    public class ValueQueue : Queue<double>
    {
    }
        
    [PluginInfo(Name = "Queue",
                Category = "Color",
                Version = "",
                Tags = ""
                )]
    public class ColorQueue : Queue<RGBAColor>
    {
    }        
    
    [PluginInfo(Name = "Queue",
                Category = "String",
                Version = "",
                Tags = ""
                )]
    public class StringQueue : Queue<string>
    {
    }        
    
    [PluginInfo(Name = "Queue",
                Category = "Transform",
                Version = "",
                Tags = ""
                )]
    public class TransformQueue : Queue<Matrix4x4>
    {
    }        
    
    [PluginInfo(Name = "Queue",
                Category = "Enumerations",
                Version = "",
                Tags = ""
                )]
    public class EnumQueue : Queue<EnumEntry>
    {
    }        
        
    
    
    
//    public class Buffer<T> : IPluginEvaluate
//    {
//        [Input("Input")]
//        ISpread<ISpread<T>> Input;
//        
//        [Input("do Insert")]
//        ISpread<bool> DoInsert;
//
//        [Input("Position")]
//        ISpread<int> Position;
//
//        
//        [Input("Frame Count", SliceMode = SliceMode.Single)]
//        ISpread<int> FrameCount;
//
//        [Output("Output")]
//        ISpread<ISpread<T>> Output;
//
//        [Output("Frames")]
//        ISpread<int> FrameSlices;
//        
//        List<T> FBuffer = new List<T>();        
//        
//        public void Evaluate(int SpreadMax)
//        {
//        	SpreadMax = Input.CombineWith(DoInsert.CombineWith(Position));
//        		
//        	for (var i = 0; i<SpreadMax; i++)
//        	{
//	        	if (DoInsert[i])
//	        	{
//	        		FBuffer.InsertRange(Position[i], Input[i]);
//	        	
//	        		// adjust positions of still to be added spreads, 
//	        		// since user specified index refers to original spread
//		        	for (var j = i+1; i<SpreadMax; j++)
//		        	{
//		        		if (Position[j] >= Position[i])
//		        			Position[j] += Input[i].SliceCount;
//		        	}
//	        	}
//        	}
//        	
//        	
//        	FBuffer.Count = FrameCount
//        	
//        	
//        	Output.SliceCount = Input.SliceCount;
//
//            for (var i = 0; i < Input.SliceCount; i++)
//            {
//            	Output[i] = Input[i];
//            }
//        }
//    }

}