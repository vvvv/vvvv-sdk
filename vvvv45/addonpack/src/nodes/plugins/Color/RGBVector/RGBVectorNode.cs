#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "RGB", Category = "Color", Version = "Split Vector", Help = "splits a color in a vector with red, green, blue and alpha components", Author = "woei")]
	#endregion PluginInfo
	public unsafe class RGBVectorSplitNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IColorIn FInPin;

		[Output("", Dimension = 4, DimensionNames = new string[]{"R","G","B","A"})]
		IValueOut FOutPin;
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			int inCount;
			double* inData, outData;
			
			FInPin.GetColorPointer(out inCount, out inData);
			
			FOutPin.SliceCount = inCount;
			FOutPin.GetValuePointer(out outData);
			
			for (int i=0; i<inCount*4; i++)
				outData[i] = inData[i];
//			Memory.Copy(outData, inData, (uint)(inCount*4*sizeof(double)));
		}
		
		#region PluginInfo
		[PluginInfo(Name = "RGB", Category = "Color", Version = "Join Vector", Help = "creates a colour out of the red, green, blue and alpha components of a vector", Author = "woei")]
		#endregion PluginInfo
		public unsafe class RGBVectorJoinNode : IPluginEvaluate
		{
			#region fields & pins
			[Input("", Dimension = 4, DimensionNames = new string[]{"R","G","B","A"}, DefaultValues = new double[]{0,1,0,1})]
			IValueFastIn FInPin;
	
			[Output("Output")]
			IColorOut FOutPin;
			#endregion fields & pins
			
			//called when data for any output pin is requested
			public void Evaluate(int spreadMax)
			{
				int inCount;
				double* inData, outData;
				
				FInPin.GetValuePointer(out inCount, out inData);
				spreadMax =(int)Math.Ceiling((double)inCount/4.0);
				
				FOutPin.SliceCount = spreadMax;
				FOutPin.GetColorPointer(out outData);
				
				int incr = 0;
				for (int i=0; i<spreadMax*4; i++)
				{
					outData[i] = inData[incr];
					incr++;
					if (incr >= inCount)
						incr=0;
				}
			}
		}
	}
}
