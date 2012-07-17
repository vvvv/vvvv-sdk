#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Collections.Generic; 

using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Input;
using VVVV.Utils.Streams;
#endregion usings

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Map", Category = "Value", Version = "Interval Advanced", Help = "alike Map (Value Interval) with binsize option to group breakpoints to different inputslices", Author = "woei")]
	public class MapIntervalAdvNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input")]
		ISpread<double> FInput;
		
		ISpread<ISpread<double>> FInBp;
        ISpread<ISpread<double>> FOutBp;
		
		[Input("Mapping", EnumName = "MapRangeMode", Order = 5)]
		ISpread<EnumEntry> FMapMode;
		
		[Output("Output")]
		ISpread<double> FOutput;
		
		[Import]
        IIOFactory FIOFactory;
		#pragma warning restore
		#endregion fields & pins
		
		public void OnImportsSatisfied()
        {
			var binSizeAttr = new InputAttribute("Bin Size");
			binSizeAttr.DefaultValue = -1;
			binSizeAttr.Order = 4;
            var binSizeIOContainer = FIOFactory.CreateIOContainer<IInStream<int>>(binSizeAttr);
            
            var inAttr = new InputAttribute("Input Breakpoint");
            inAttr.Order = 2;
            FInBp = new InputBinSpread<double>(FIOFactory, inAttr, binSizeIOContainer);
            
            var outAttr = new InputAttribute("Output Breakpoint");
            outAttr.Order = 3;
            FOutBp = new InputBinSpread<double>(FIOFactory, outAttr, binSizeIOContainer);
        } 
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FInBp.Sync();
			FOutBp.Sync();
			
			SpreadMax = FInput.CombineWith(FInBp).CombineWith(FOutBp).CombineWith(FMapMode);
			
			FOutput.SliceCount = SpreadMax;
			for (int i=0; i<SpreadMax; i++)
			{
				if (FOutBp[i].SliceCount == 1)  //if only one in- & outbreakpoint output outbreakpoint
				{
					FOutput[i] = FOutBp[i][0];
				}
				else if (FOutBp[i].SliceCount>0) //apply mapinterval only if more than one in-/outbreakpoint
				{
					MapInterval m = new MapInterval(FInBp[i].ToList(), FOutBp[i].ToList());
	    			FOutput[i] = m.DoMap(FInput[i], FMapMode[i].Index);
				}
			}
		}
	}
}
