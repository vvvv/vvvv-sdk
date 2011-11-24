using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	[ComVisible(false)]
	public abstract class ValueOutputPin<T> : ValuePin<T> where T: struct
	{
		protected readonly IValueOut FValueOut;
		unsafe private double* FDestination;
		private bool FChanged = true;
		
		public ValueOutputPin(IPluginHost host, OutputAttribute attribute, double minValue, double maxValue, double stepSize)
			: base(host, attribute, minValue, maxValue, stepSize)
		{
			host.CreateValueOutput(FName, 1, null, FSliceMode, FVisibility, out FValueOut);
			FValueOut.SetSubType(FMinValue, FMaxValue, FStepSize, attribute.DefaultValue, FIsBang, FIsToggle, FIsInteger);
			base.InitializeInternalPin(FValueOut);
		}
		
		unsafe protected abstract void CopyFromBuffer(T[] buffer, double* destination, int startIndex, int length);
		
		unsafe public override void Update()
		{
			base.Update();
			
			if (FAttribute.SliceMode != SliceMode.Single)
				FValueOut.SliceCount = FSliceCount;
			
			if (FChanged && FSliceCount > 0)
			{
				FValueOut.GetValuePointer(out FDestination);
				CopyFromBuffer(FBuffer, FDestination, 0, FSliceCount);
			}
			
			FChanged = false;
		}
		
        public override int SliceCount
        {
            get 
            { 
                return base.SliceCount; 
            }
            set 
            {
                FChanged = true;
                base.SliceCount = value; 
            }
        }
        
        public override T this[int index] 
        {
            get
            { 
                return base[index]; 
            }
            set 
            { 
                FChanged = true;
                base[index]  = value; 
            }
        }
	}
}
