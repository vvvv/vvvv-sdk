using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double,
	/// Vector2D, Vector3D, Vector4D
	/// </summary>
	public abstract class ValueInputPin<T> : InputPin<T> where T: struct
	{
		protected IValueIn FValueIn;
		protected double[] FData;
		protected int FDimension;
		
		public ValueInputPin(IPluginHost host, InputAttribute attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			
			LoadDefaultValues(type, attribute, out FDimension, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueInput(attribute.Name, FDimension, null, attribute.SliceMode, attribute.Visibility, out FValueIn);
			switch (FDimension)
			{
				case 2:
					FValueIn.SetSubType2D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], false, false, isInteger);
					break;
				case 3:
					FValueIn.SetSubType3D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], false, false, isInteger);
					break;
				case 4:
					FValueIn.SetSubType4D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], false, false, isInteger);
					break;
				default:
					FValueIn.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, false, false, isInteger);
					break;
			}
			
			FData = new double[FDimension * 1];
		}
		
		public override IPluginIn PluginIn
		{
			get
			{
				return FValueIn;
			}
		}
		
		public override int SliceCount 
		{
			get 
			{
				return base.SliceCount;
			}
			set 
			{
				if (FData.Length != value)
					FData = new double[value * FDimension];
				
				base.SliceCount = value;
			}
		}
		
		unsafe public override void Update()
		{
			if (FValueIn.PinIsChanged)
			{
				int sliceCount;
				double* source;
				
				FValueIn.GetValuePointer(out sliceCount, out source);
				
				if (sliceCount != FData.Length)
					FData = new double[sliceCount * FDimension];
				
				Marshal.Copy(new IntPtr(source), FData, 0, sliceCount * FDimension);
			}
		}
	}
}
