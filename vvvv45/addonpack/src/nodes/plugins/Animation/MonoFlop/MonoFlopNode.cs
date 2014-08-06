#region usings
using System;

using VVVV.PluginInterfaces.V2;
#endregion usings

namespace VVVV.Nodes
{
	public abstract class MonoFlop : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Set", IsBang = true)]
		ISpread<bool> FSet;
		
		[Input("Reset", IsBang = true)]
		ISpread<bool> FReset;
		
		[Input("Retriggerable", DefaultBoolean = true, Order = 4)]
		ISpread<bool> FReTrig;

		[Output("Output")]
		ISpread<bool> FOut;
		
		[Output("Inverse Output")]
		ISpread<bool> FInvOut;
		
		private Spread<double> FBuffer = new Spread<double>(0);
		#pragma warning restore
		#endregion fields & pins

		protected abstract double Increment();
		protected abstract double Duration(int slice);
		
		public virtual void Evaluate(int spreadMax)
		{
			FOut.SliceCount = spreadMax;
			FInvOut.SliceCount = spreadMax;
			
			FBuffer.SliceCount = spreadMax;
			for (int i = 0; i < spreadMax; i++)
			{
				if (FOut[i])
				{
					FBuffer[i] += Increment();
					
					if (FSet[i] && FReTrig[i])
					{
						FBuffer[i] = 0;
					}
					
					if (FBuffer[i] >= Duration(i) || FReset[i])
					{
						FBuffer[i] = 0;
						FOut[i] = false;
						FInvOut[i] = true;
					}
				}
				else if (FSet[i])
				{
					FOut[i] = true;
					FInvOut[i] = false;
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "MonoFlop", 
				Category = "Animation", 
				Version = "Framebased", 
				Help = "Sets the output to 1 for a predefined number of frames.", 
				Author = "vvvv group, woei",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class MonoFlopFramebased : MonoFlop
	{
		#region fields & pins
		[Input("Frames", MinValue = 0, DefaultValue = 1, Order = 3)]
		public ISpread<int> FDuration;
		#endregion fields & pins
		
		protected override double Increment()
		{
			return 1;
		}
		
		protected override double Duration(int slice)
		{
			return (double)FDuration[slice];
		}
	}
}