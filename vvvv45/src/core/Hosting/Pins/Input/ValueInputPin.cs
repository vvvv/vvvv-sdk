using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class ValueInputPin<T> : ValuePin<T> where T: struct
	{
		protected IValueFastIn FValueFastIn;
		
		unsafe private double* FSource;
		
		public ValueInputPin(IPluginHost host, InputAttribute attribute, double minValue, double maxValue, double stepSize)
			: base(host, attribute, minValue, maxValue, stepSize)
		{
			host.CreateValueFastInput(FName, 1, null, FSliceMode, FVisibility, out FValueFastIn);
			FValueFastIn.SetSubType(FMinValue, FMaxValue, FStepSize, FDefaultValue, FIsBang, FIsToggle, FIsInteger);
			base.InitializeInternalPin(FValueFastIn);
		}
		
		unsafe protected abstract void CopyToBuffer(T[] buffer, double* source, int startingIndex, int length);
		
		unsafe public override void Update()
		{
			int length;
			
			FValueFastIn.GetValuePointer(out length, out FSource);
			
			SliceCount = length;
			
			if (!FLazy && FSliceCount > 0)
				CopyToBuffer(FBuffer, FSource, 0, length);
			
			base.Update();
		}
		
		unsafe protected override void DoLoad(int index, int length)
		{
			CopyToBuffer(FBuffer, FSource, index, length);
		}
	}
}
