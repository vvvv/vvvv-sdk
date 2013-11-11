#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.Utils.BitOperators
{

	#region PluginInfo
	[PluginInfo(Name = "MSB", Category = "Value Bitwise", Help = "Get the most significant byte from an integer.")]
	#endregion PluginInfo
	public class MSB : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Input("Number of Bits", DefaultValue = 7, MaxValue = 8, MinValue=0, Visibility = PinVisibility.Hidden,IsSingle=true)]
		IDiffSpread<int> NumBits;
		
		[Output("MSB")]
		ISpread<int> FOutput;

		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged && NumBits.IsChanged) return;
			FOutput.SliceCount = SpreadMax;
			int shift = 8-NumBits[0]; int mask  = 0xff>>shift;
      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = (FInput[i] >> NumBits[i]) & mask;
      }
		}
	}
	
	
	#region PluginInfo
  [PluginInfo(Name = "LSB", Category = "Value Bitwise", Help = "Get the least significant byte.")]
	#endregion PluginInfo
	public class LSB : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;

    [Input("Number of Bits", DefaultValue = 7, MaxValue = 8, MinValue = 0, Visibility = PinVisibility.Hidden, IsSingle = true)]
    IDiffSpread<int> NumBits;
		
		[Output("LSB")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged && NumBits.IsChanged) return;
			FOutput.SliceCount = SpreadMax;
      int shift = 8 - NumBits[0]; int mask = 0xff >> shift;
      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = FInput[i] & mask;
      }
		}
	}
	
	#region PluginInfo
  [PluginInfo(Name = "LSB+MSB", Category = "Value Bitwise", Help = "Get an integer from a least and a most significatn byte.")]
	#endregion PluginInfo
	public class PackLSBMSB : IPluginEvaluate
	{
		#region fields & pins
		[Input("LSB", DefaultValue = 0)]
		IDiffSpread<int> LSB;

		[Input("MSB", DefaultValue = 0)]
		IDiffSpread<int> MSB;

    [Input("Number of Bits", DefaultValue = 7, MaxValue = 8, MinValue = 0, Visibility = PinVisibility.Hidden, IsSingle = true)]
    IDiffSpread<int> NumBits;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!LSB.IsChanged && !MSB.IsChanged && !NumBits.IsChanged) return;

			FOutput.SliceCount = Math.Max(LSB.SliceCount,MSB.SliceCount);

			int shift = NumBits[0];
      int mask  = 0xff>>shift;

			for (int i = 0; i < FOutput.SliceCount; i++)
			{
				FOutput[i] = ((MSB[i] & mask) << NumBits[i]) | (LSB[i]&mask);
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "<<", Category = "Value Bitwise", Help = "Shift bits to the left by a given value.")]
	#endregion PluginInfo
	public class Shiftleft : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		ISpread<int> FInput;
		
		[Input("Steps", DefaultValue = 0)]
		ISpread<int> StepInput;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = FInput[i] << StepInput[i];
      }
		}
	}
	
	#region PluginInfo
  [PluginInfo(Name = ">>", Category = "Value Bitwise", Help = "Shift bits to the right by a given value.")]
	#endregion PluginInfo
	public class Shiftright : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Input("Steps", DefaultValue = 0)]
		IDiffSpread<int> StepInput;

    [Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged && !StepInput.IsChanged) return;
			FOutput.SliceCount = SpreadMax;
      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = FInput[i] >> StepInput[i];
      }
		}
	}
	
	#region PluginInfo
  [PluginInfo(Name = "&", Category = "Value Bitwise", Help = "Get the bitwise AND value", Tags = "")]
	#endregion PluginInfo
	public class BitAND : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Input("Input 2", DefaultValue = 0)]
		IDiffSpread<int> FInput2;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged && !FInput2.IsChanged) return;
			FOutput.SliceCount = SpreadMax;

      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = FInput[i] & FInput2[i];
      }
			
		}
	}
	#region PluginInfo
  [PluginInfo(Name = "&", Category = "Value Bitwise", Version = "Spectral", Help = "Get the bitwise AND value.")]
	#endregion PluginInfo
	public class BitANDSpectral : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged) return;
			FOutput.SliceCount = 1;
			int result = 0;
      for (int i = 0; i < SpreadMax; i++)
      {
        result &= FInput[i];
      }
			FOutput[0] = result;
		}
	}
	
	#region PluginInfo
  [PluginInfo(Name = "|", Category = "Value Bitwise", Help = "Get the bitwise OR value.")]
	#endregion PluginInfo
	public class BitOR : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Input("Input 2", DefaultValue = 0)]
		IDiffSpread<int> FInput2;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged && !FInput2.IsChanged) return;
			FOutput.SliceCount = SpreadMax;

      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = FInput[i] | FInput2[i];
      }
			
		}
	}

	#region PluginInfo
  [PluginInfo(Name = "|", Category = "Value Bitwise", Version = "Spectral", Help = "Get the bitwise OR value.")]
	#endregion PluginInfo
	public class BitOrSpectral : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged) return;
			FOutput.SliceCount = 1;
			int result = 0;
      for (int i = 0; i < SpreadMax; i++)
      {
        result |= FInput[i];
      }
			FOutput[0] = result;
			
		}
	}

	
	#region PluginInfo
  [PluginInfo(Name = "~", Category = "Value Bitwise", Help = "Get the bitwise NOT of a value", Tags = "")]
	#endregion PluginInfo
	public class BitNOT : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged) return;
			FOutput.SliceCount = SpreadMax;
      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = ~FInput[i];
      }
		}
	}
	
	#region PluginInfo
  [PluginInfo(Name = "^", Category = "Value Bitwise", Help = "Get the bitwise XOR value.")]
	#endregion PluginInfo
	public class BitXOR : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Input("Input 2", DefaultValue = 0)]
		IDiffSpread<int> FInput2;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged && !FInput2.IsChanged) return;
			FOutput.SliceCount = SpreadMax;
      for (int i = 0; i < SpreadMax; i++)
      {
        FOutput[i] = FInput[i] ^ FInput2[i];
      }
		}
	}
	
	#region PluginInfo
  [PluginInfo(Name = "FastBit", Category = "Value Bitwise", Help = "Convert an integer into their bit sequence")]
	#endregion PluginInfo
	public class FastBit : IPluginEvaluate
	{
    public enum Endianess
    {
      Little_Endian, BigEndian
    }

		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		IDiffSpread<int> FInput;
		
		[Input("BitSize", DefaultValue = 8, IsSingle = true)]
		IDiffSpread<int> BitSize;

    [Input("Endianess", DefaultValue = 8, IsSingle = true, Visibility = PinVisibility.Hidden)]
    IDiffSpread<Endianess> Direction;
		
		[Output("Output")]
		ISpread<int> FOutput;
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged && !BitSize.IsChanged && !Direction.IsChanged) return;
			FOutput.SliceCount = SpreadMax * BitSize[0];
			for (int i = 0; i < FOutput.SliceCount; i++)
			{
        for (int bi = 0; bi < BitSize[i]; bi++)
        {
          int index = Direction[0] == Endianess.Little_Endian ? bi : BitSize[i] - 1 - bi;
          FOutput[(i * BitSize[i]) + index] = (FInput[i] >> bi) & 0x01;
        }
			}
		}
	}
	
	
}
