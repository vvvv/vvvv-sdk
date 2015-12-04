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

		[Output("Color")]
		public ISpread<RGBAColor> FColorOut;
		
		[Output("Value")]
		public ISpread<double> FValueOut;
		
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
				FValueOut[0] = x;				
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
				FColorOut[0] = x;				
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

	public class MyColorAndValueData : IColorData, IValueData
	{
		public RGBAColor Color;
		public double Value;
		
		public void GetColor(int index, out RGBAColor color)
		{
			color = Color;
		}
		
		public void GetValue(int index, out double value)
		{
			value = Value;
		}
	}
	
	[PluginInfo(Name = "TestBSource", Category = "Node")]
	public class VariantTestBSourceNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		[Input("X")]
		public INodeIn FX; // node pin without any subtype info set
		
		[Output("Value And Color")]
		public INodeOut FDataOut;		
		
		[Import()]
        public ILogger FLogger;		
		
		MyColorAndValueData FData;
		
		public void OnImportsSatisfied()
		{
			FDataOut.SetSubType(
				new Guid[]{ 					
					typeof(IValueData).GUID,
					typeof(IColorData).GUID, 					
				 },
				"Value And Color mainly");
			
		    FData = new MyColorAndValueData();				
			FDataOut.SetInterface(FData);
		}		
		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			object myData;
			FX.GetUpstreamInterface(out myData);
			
			if (myData == null)
				return;
			
			int upSlice;
			FX.GetUpsreamSlice(0, out upSlice);
						
			var myValueData = myData as IValueData;
			if (myValueData != null)
			{
				double x;
				myValueData.GetValue(upSlice, out x);
				
				FData.Value = x;							
				return;
			}
						
			var myColorData = myData as IColorData;
			if (myColorData != null)
			{
				RGBAColor x;
				myColorData.GetColor(upSlice, out x);
				
				FData.Color = x;						
				return;
			}
		}
	}
	
	[PluginInfo(Name = "TestB", Category = "Node")]
	public class VariantTestBNode : IPluginEvaluate
	{
		[Input("X")]
		public INodeIn FX; // node pin without any subtype info set		

		[Output("Color")]
		public ISpread<RGBAColor> FColorOut;
		
		[Output("Value")]
		public ISpread<double> FValueOut;		
		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			object myData;
			FX.GetUpstreamInterface(out myData);
			
			if (myData == null)
				return;
			
			int upSlice;
			FX.GetUpsreamSlice(0, out upSlice);

			var myValueData = myData as IValueData;
			if (myValueData != null)
			{
				double x;
				myValueData.GetValue(upSlice, out x);				
				FValueOut[0] = x;
			}			
			
			var myColorData = myData as IColorData;
			if (myColorData != null)
			{
				RGBAColor x;
				myColorData.GetColor(upSlice, out x);						
				FColorOut[0] = x;				
			}			
		}
	}	
}
