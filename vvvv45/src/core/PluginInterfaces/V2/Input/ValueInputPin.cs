using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double,
	/// Vector2D, Vector3D, Vector4D
	/// </summary>
	public abstract class ValueInputPin<T> : Pin<T>, IPinUpdater where T: struct
	{
		protected IValueFastIn FValueFastIn;
		
		protected int FSliceCount = 1;
		protected double[] FData;
		protected int FDimension;
		
		public ValueInputPin(IPluginHost host, InputAttribute attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			
			LoadDefaultValues(type, attribute, out FDimension, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueFastInput(attribute.Name, FDimension, null, attribute.SliceMode, attribute.Visibility, out FValueFastIn);
			switch (FDimension)
			{
				case 2:
					FValueFastIn.SetSubType2D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], false, false, isInteger);
					break;
				case 3:
					FValueFastIn.SetSubType3D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], false, false, isInteger);
					break;
				case 4:
					FValueFastIn.SetSubType4D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], false, false, isInteger);
					break;
				default:
					FValueFastIn.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, false, false, isInteger);
					break;
			}
			
			FValueFastIn.SetPinUpdater(this);
			
			FData = new double[FDimension * 1];
		}
		
		public override IPluginIO PluginIO 
		{
			get 
			{
				return FValueFastIn;
			}
		}
		
		public override int SliceCount
		{
			get
			{
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
					FData = new double[value * FDimension];
				
				FSliceCount = value;
			}
		}
		
		unsafe public override void Update()
		{
			int sliceCount;
			double* source;
			
			FValueFastIn.GetValuePointer(out sliceCount, out source);
			SliceCount = sliceCount / FDimension;
			Marshal.Copy(new IntPtr(source), FData, 0, FData.Length);
		}
	}
}
