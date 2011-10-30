#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes
{
    public enum ThroughPutStyle { Copy, CopySlices}; //, DeepCopy };
      
    
    public class ThroughPutTest<T>: IPluginEvaluate
    {    
    	#pragma warning disable 649
        [Input("Values")] ISpread<T> Values;        
        [Input("DiffValues")] IDiffSpread<T> DiffValues;

        [Input("Spreads")] ISpread<ISpread<T>> Spreads;
        [Input("DiffSpreads")] IDiffSpread<ISpread<T>> DiffSpreads;

//        [Input("Spreadss")] ISpread<ISpread<ISpread<T>>> Spreadss;
        
//        [Input("Copy Style", DefaultEnumEntry = "CopySlices")] ISpread<ThroughPutStyle> Style;
        
        [Output("Values Copy")] ISpread<T> ValuesCopy;
        [Output("DiffValues Copy")] ISpread<T> DiffValuesCopy;
        [Output("DiffValues Changed")] ISpread<bool> DiffValuesChanged;                

        [Output("Spreads Copy")] ISpread<ISpread<T>> SpreadsCopy;
        [Output("DiffSpreads Copy")] ISpread<ISpread<T>> DiffSpreadsCopy;
        [Output("DiffSpreads Changed")] ISpread<bool> DiffSpreadsChanged;                

        [Output("Spreads As String")] ISpread<string> SpreadsAsString;
//        [Output("Spreadss Copy")] ISpread<ISpread<ISpread<T>>> SpreadssCopy;
		#pragma warning restore
        
        
        protected void PutThrough<U>(ISpread<U> output, ISpread<U> input)        
        {
        	switch (ThroughPutStyle.CopySlices)//(Style[0])
            {
//                case ThroughPutStyle.Copy:
//                    output = input;
//                    break;
                    
                case ThroughPutStyle.CopySlices:
                    output.SliceCount = input.SliceCount;
                    
                	for (int i = 0; i < input.SliceCount; i++)
                		output[i] = input[i];               	                    		
                	break;	
                
//                case ThroughPutStyle.DeepCopy:
//                    output.SliceCount = input.SliceCount;
//               
//                    if (output is Spread<V>) 
//                        for (int i = 0; i < input.SliceCount; i++)
//                		    PutThrough(output[i], input[i]);               	                    		
//                    else    
//                        for (int i = 0; i < input.SliceCount; i++)
//                            output[i] = input[i];               	                    		
            }
                                
            output.SliceCount = input.SliceCount;
            
        	for (int i = 0; i < input.SliceCount; i++)
        		output[i] = input[i];               	                    		
        }                              
 
        public void DiffPutThrough<U>(ISpread<U> output, IDiffSpread<U> input, ISpread<bool> changed)
        {
            PutThrough(output, input);
            changed.SliceCount = 1;
      		changed[0] = input.IsChanged;        	
        }
         
        public void Evaluate(int SpreadMax)
        {    
            // general info: only inputs and ouputs are checked by throughput nodes       
            // check functionality of ISpread<T> and IDiffSpread<T>
            PutThrough(ValuesCopy, Values);
            DiffPutThrough(DiffValuesCopy, DiffValues, DiffValuesChanged);
 
            // check functionality of ISpread<ISpread<T>> and IDiffSpread<ISpread<T>> as "bin spreads"
            PutThrough(SpreadsCopy, Spreads);
            DiffPutThrough(DiffSpreadsCopy, DiffSpreads, DiffSpreadsChanged);
            
            // check functionality of ISpread<ISpread<T>> and IDiffSpread<ISpread<T>> as "pin groups"

            // special bonus check: spread as string
            SpreadsAsString.SliceCount = Spreads.SliceCount;            
        	for (int i = 0; i < Spreads.SliceCount; i++)
        		SpreadsAsString[i] = Spreads[i].AsString();
             
             
            // special bonus check: complex spread construct will be supported by node pins
 //            PutThrough(SpreadssCopy, Spreadss);
       }
    }

         
	[PluginInfo(Name = "Through",
                Category = "Test",
                Version = "bool",
                Tags = "")]
    public class BoolThroughPutTest: ThroughPutTest<bool>, IPluginEvaluate
    { 
    }

	[PluginInfo(Name = "Through",
                Category = "Test",
                Version = "int",
                Tags = "")]
    public class IntThroughPutTest: ThroughPutTest<int>, IPluginEvaluate
    { 
    }
    
	[PluginInfo(Name = "Through",
                Category = "Test",
                Version = "float",
                Tags = "")]
    public class FloatThroughPutTest: ThroughPutTest<float>, IPluginEvaluate
    { 
    }

	[PluginInfo(Name = "Through",
                Category = "Test",
                Version = "double",
                Tags = "")]
    public class DoubleThroughPutTest: ThroughPutTest<double>, IPluginEvaluate
    { 
    }
    
	[PluginInfo(Name = "Through",
                Category = "Test",
                Version = "string",
                Tags = "")]
    public class StringThroughPutTest: ThroughPutTest<string>, IPluginEvaluate
    { 
    }

    [PluginInfo(Name = "Through",
                Category = "Test",
                Version = "Transform",
                Tags = "")]
    public class TransformThroughPutTest: ThroughPutTest<Matrix4x4>, IPluginEvaluate
    { 
    }
}