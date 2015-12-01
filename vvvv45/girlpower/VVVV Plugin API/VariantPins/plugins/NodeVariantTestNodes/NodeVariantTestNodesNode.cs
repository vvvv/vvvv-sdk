#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	public class SomeType
	{
		
	}
	
	[PluginInfo(Name = "OutputSomeType", Category = "Node")]
	public class OutputSomeTypeNode : IPluginEvaluate
	{	
		[Output("Output")]
		public ISpread<SomeType> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			FOutput.SliceCount = 1;
			FOutput[0] = new SomeType();
		}
	}	
	
	
	[PluginInfo(Name = "TestA", Category = "Node")]
	public class VariantTestANode : IPluginEvaluate
	{
		[Input("X")]
		public INodeIn FX; // node pin without any subtype info set
		
		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
        public ILogger FLogger;		
		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			object myData;
			FX.GetUpstreamInterface(out myData);
			
			if (myData == null)
			{			
				FOutput[0] = "not connected";		
				return;
			}
			
			int upSlice;
			FX.GetUpsreamSlice(0, out upSlice);
						
			var myValueData = myData as IValueData;
			if (myValueData != null)
			{
				double x;
				myValueData.GetValue(upSlice, out x);
				FOutput[0] = "value: " + x.ToString();
				return;
			}
			
			var myStringData = myData as IStringData;
			if (myStringData != null)
			{
				string x;
				myStringData.GetString(upSlice, out x);
				FOutput[0] = "string: " + x.ToString();
				return;
			}
			
			var myColorData = myData as IColorData;
			if (myColorData != null)
			{
				RGBAColor x;
				myColorData.GetColor(upSlice, out x);
				FOutput[0] = "color: " + x.ToString();
				return;
			}
			
			var myRawData = myData as IRawData;
			if (myRawData != null)
			{
				IStream x;
				myRawData.GetData(upSlice, out x);
				FOutput[0] = "raw: " + x.ToString();
				return;
			}		
									
			var myFooData = myData as IEnumerable<object>;
			if (myFooData != null && myFooData.Any())
			{
				var spread = myFooData.ToSpread();
				FOutput[0] = spread[upSlice].ToString();
				return;
			}
	
			FLogger.Log(LogType.Debug, myData.ToString());
			FOutput[0] = "unknown data";
		}
	}
}
