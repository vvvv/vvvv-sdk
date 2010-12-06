using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class DiffValueInputPin<T> : DiffValuePin<T>, IDiffSpread<T> where T: struct
	{
		protected IValueIn FValueIn;
		
		unsafe private double* FSource;
		
		public DiffValueInputPin(IPluginHost host, InputAttribute attribute, double minValue, double maxValue, double stepSize)
			: base(host, attribute, minValue, maxValue, stepSize)
		{
			host.CreateValueInput(FName, 1, null, FSliceMode, FVisibility, out FValueIn);
			FValueIn.SetSubType(FMinValue, FMaxValue, FStepSize, FDefaultValue, FIsBang, FIsToggle, FIsInteger);
			base.InitializeInternalPin(FValueIn);
		}
		
		unsafe protected abstract void CopyToBuffer(T[] buffer, double* source, int startingIndex, int length);
		
		unsafe public override void Update()
		{
			if (IsChanged)
			{
				int length;
				
				FValueIn.GetValuePointer(out length, out FSource);
				
				SliceCount = length;
				
				if (!FLazy && FSliceCount > 0)
					CopyToBuffer(FBuffer, FSource, 0, length);
			}
			
			base.Update();
		}
		
		unsafe protected override void DoLoad(int index, int length)
		{
			CopyToBuffer(FBuffer, FSource, index, length);
		}
		
		public override bool IsChanged
		{
			get
			{
				return FValueIn.PinIsChanged;
			}
		}
	}
}
