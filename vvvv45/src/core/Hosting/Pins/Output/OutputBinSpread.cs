using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class OutputBinSpread<T> : BinSpread<T>, IDisposable
	{
		protected Pin<int> FBinSize;
		protected Pin<T> FSpreadPin;
		protected bool FSpreadsBuilt;
		protected bool FChanged;
		protected int FUpdateCount;
		protected string FBinName;
		
		public OutputBinSpread(IPluginHost host, OutputAttribute attribute)
			: base(attribute)
		{
			FBinName = attribute.BinName;
			
			//data pin
			FSpreadPin = PinFactory.CreatePin<T>(host, attribute);
			FSpreadPin.Updated += AnyUpdated;
			
			//bin size pin
			if (FBinName == " Bin Size")
			  FBinName = attribute.Name+FBinName;
			
			var att = new OutputAttribute(FBinName);
			att.DefaultValue = 1;
			att.Visibility = attribute.BinVisibility;
			att.Order = attribute.BinOrder;
			FBinSize = new IntOutputPin(host, att);
			FBinSize.Updated += AnyUpdated;
		}
		
		public virtual void Dispose()
		{
		    FSpreadPin.Updated -= AnyUpdated;
		    FBinSize.Updated -= AnyUpdated;
		    FSpreadPin.Dispose();
		    FBinSize.Dispose();
		}

		void AnyUpdated(object sender, EventArgs args)
		{
			if (FUpdateCount == 0)
				BuildSpreads();
			
			FUpdateCount++;

			if (FUpdateCount >= 2)
				FUpdateCount = 0;
		}
		
		public override int SliceCount 
		{
			get 
			{ 
				return base.SliceCount; 
			}
			set 
			{ 
				base.SliceCount = value;
				FChanged = true;				
			}
		}
		
		public override ISpread<T> this[int index] 
		{
			get
			{ 
				return base[index];
			}
			set 
			{
				base[index] = value;
				FChanged = true;
			}
		}

		void BuildSpreads()
		{
			
			if(FChanged)
			{
				FBinSize.SliceCount = SliceCount;
				
				int count = 0;
				for(int i = 0; i < SliceCount; i++)
				{
					var c = this[i].SliceCount;
					count += c;
					FBinSize[i] = c;
				}
				
				FSpreadPin.SliceCount = count;
				
				var outputBuffer = FSpreadPin.Buffer;
				int offset = 0;
				for(int i = 0; i < SliceCount; i++)
				{
					var spread = this[i];
					
					for(int j = 0; j < spread.SliceCount; j++)
						outputBuffer[offset + j] = spread[j];
					
					offset += spread.SliceCount;
				}
			}
			
			FChanged = false;
		}
	}
}
