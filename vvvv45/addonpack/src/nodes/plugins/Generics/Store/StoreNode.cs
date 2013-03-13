#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	
	public class Store<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
    	protected ISpread<T> FIn;
    	[Input("Bin Size", DefaultValue=1)]
    	protected ISpread<int> FBinSize;
    	[Input("Index")]
    	protected ISpread<int> FId;
    	
    	[Input("Set")]
    	protected IDiffSpread<bool> FSet;
    	[Input("Remove")]
    	protected IDiffSpread<bool> FRemove;
    	[Input("Insert")]
    	protected IDiffSpread<bool> FInsert;
    	
    	[Input("Flush", IsBang = true, IsSingle = true, Order=10)]
    	protected IDiffSpread<bool> FFlush;
    	[Input("Clear", IsBang = true, IsSingle = true, Order=11)]
    	protected IDiffSpread<bool> FClear;
    	
    	//output pin declaration
    	[Output("Output")]
    	protected ISpread<T> FOut;
    	
		
		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FClear[0])
				FOut.SliceCount=0;
			
			if (FFlush[0])
				FOut.AssignFrom(FIn);

			if (PinChanged())
			{
				Spread<Spread<T>> buffer = new Spread<Spread<T>>(0);
				
				int incr = 0, sum = 0;
				while (!(sum>=FOut.SliceCount && (incr%FBinSize.SliceCount)==0))
				{
					buffer.Add((Spread<T>)FOut.GetRange(sum, FBinSize[incr]));
					sum += FBinSize[incr];
					incr++;
				}
				
				incr = 0;
				for (int i=0; i<FId.SliceCount; i++)
				{
					int binSize = 1;
					try
					{
						binSize = buffer[FId[i]].SliceCount;
					}
					catch
					{
						binSize = FBinSize[FId[i]];
					}
					Alter(i, incr, binSize, ref buffer);		
					incr+=binSize;
				}
				
				FOut.SliceCount=0;
				foreach (Spread<T> t in buffer)
					FOut.AddRange(t);
				
			}
		}
		
		public virtual bool PinChanged()
		{
			return FSet.Any(x => x == true) || FRemove.Any(x => x == true) || FInsert.Any(x => x == true);
			//return FSet.IsChanged || FRemove.IsChanged || FInsert.IsChanged;
		}
		
		public virtual void Alter(int i, int incr, int binSize, ref Spread<Spread<T>> buffer)
		{
			if (FSet[i] && buffer.SliceCount > 0)
				buffer[FId[i]]=(Spread<T>)FIn.GetRange(incr,binSize);
			if (FRemove[i] && buffer.SliceCount > 0)
				buffer.RemoveAt(FId[i]);
			if (FInsert[i])
			{
				int id = VMath.Zmod(FId[i], buffer.SliceCount+1);				
				if (buffer.SliceCount>0 || id<buffer.SliceCount)
					buffer.Insert(id,(Spread<T>)FIn.GetRange(incr,binSize));	
				else
					buffer.Add((Spread<T>)FIn.GetRange(incr,binSize));		
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Spreads", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreValue: Store<double> 
	{
		[Input("Increment", IsBang = true, Order=1)]
    	private IDiffSpread<bool> FIncrement;
		
		public override bool PinChanged()
		{
			return base.PinChanged() || FIncrement.Any(x => x == true);
		}
		
		public override void Alter(int i, int incr, int binSize, ref Spread<Spread<double>> buffer)
		{
			if (FIncrement[i] && buffer.SliceCount > 0)
			{
				for (int s=0; s<binSize; s++)
					buffer[FId[i]][s]+=FIn[incr+s];
			}
			base.Alter(i, incr, binSize, ref buffer);
		}
		
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "String", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreString: Store<string> 
	{
		[Input("Increment", IsBang = true, Order=1)]
    	private IDiffSpread<bool> FIncrement;
    	
    	public override bool PinChanged()
		{
			return base.PinChanged() || FIncrement.Any(x => x == true);
		}
    	
    	public override void Alter(int i, int incr, int binSize, ref Spread<Spread<string>> buffer)
		{
			if (FIncrement[i] && buffer.SliceCount > 0)
			{
				for (int s=0; s<binSize; s++)
					buffer[FId[i]][s]+=FIn[incr+s];
			}
			base.Alter(i, incr, binSize, ref buffer);
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Color", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreColor: Store<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Transform", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreTransform: Store<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Enumerations", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreEnum: Store<EnumEntry> {}
	
	#region PluginInfo
	[PluginInfo(Name = "Store", Category = "Raw", Help = "Stores a spread and sets/removes/inserts slices", Tags = "spread, set, remove, insert", Author = "woei", AutoEvaluate = true)]
	#endregion PluginInfo
	public class StoreRaw: Store<System.IO.Stream> {}
}
