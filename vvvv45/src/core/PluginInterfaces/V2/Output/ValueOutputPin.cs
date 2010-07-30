using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class ValueOutputPin<T> : Pin<T>, IPinUpdater where T: struct
	{
		protected IValueOut FValueOut;
		protected double[] FData;
		protected int FDimension;
		protected int FSliceCount;
		
		public ValueOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			
			LoadDefaultValues(type, attribute, out FDimension, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueOutput(attribute.Name, FDimension, null, attribute.SliceMode, attribute.Visibility, out FValueOut);
			switch (FDimension)
			{
				case 2:
					FValueOut.SetSubType2D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], false, false, isInteger);
					break;
				case 3:
					FValueOut.SetSubType3D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], false, false, isInteger);
					break;
				case 4:
					FValueOut.SetSubType4D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], false, false, isInteger);
					break;
				default:
					FValueOut.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, false, false, isInteger);
					break;
			}
			FValueOut.SetPinUpdater(this);
			
			FData = new double[FDimension * 1];
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FSliceCount;
			}
			set 
			{
				if (FData.Length != value)
					FData = new double[value * FDimension];
				
				FSliceCount = value;
			}
		}
		
		unsafe public override void Update()
		{
			double* destination;
			FValueOut.GetValuePointer(out destination);
			
			Marshal.Copy(FData, 0, new IntPtr(destination), FData.Length);
		}
	}
}
