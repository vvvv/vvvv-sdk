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
	public abstract class ObservableValueInputPin<T> : ObservablePin<T>, IPinUpdater where T: struct
	{
		protected IValueIn FValueIn;
		
		protected int FSliceCount;
		protected double[] FData;
		protected int FDimension;
		
		public ObservableValueInputPin(IPluginHost host, InputAttribute attribute)
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
			
			FValueIn.SetPinUpdater(this);
			
			SliceCount = 1;
		}
		
		public override IPluginIO PluginIO
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
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
					FData = new double[value * FDimension];
				
				FSliceCount = value;
			}
		}
		
		public override bool IsChanged
		{
			get
			{
				return FValueIn.PinIsChanged;
			}
		}
		
		unsafe public override void Update()
		{
			if (IsChanged)
			{
				int sliceCount;
				double* source;
				
				FValueIn.GetValuePointer(out sliceCount, out source);
				
				var moduloResult = sliceCount % FDimension;
				if (moduloResult != 0)
					SliceCount = sliceCount / FDimension + 1;
				else
					SliceCount = sliceCount / FDimension;
				
				Marshal.Copy(new IntPtr(source), FData, 0, sliceCount);
				
				// Fill end of FData with values from start.
				Array.Copy(FData, 0, FData, sliceCount, FData.Length - sliceCount);
			}
			
			base.Update();
		}
	}
}
