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
	public abstract class ValueOutputPin<T> : Pin<T>, IPinUpdater where T: struct
	{
		protected IValueOut FValueOut;
		new protected double[] FData;
		protected int FDimension;
		
		public ValueOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			bool isBool = type == typeof(bool);
			
			LoadDefaultValues(type, attribute, out FDimension, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueOutput(attribute.Name, FDimension, attribute.DimensionNames, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FValueOut);
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
					FValueOut.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, isBool && attribute.IsBang, isBool && !attribute.IsBang, isInteger);
					break;
			}
			
			base.Initialize(FValueOut);
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
				{
					FData = new double[value * FDimension];
				
					FSliceCount = value;
				
					if (FAttribute.SliceMode != SliceMode.Single)
						FValueOut.SliceCount = value;
				}
			}
		}
		
		unsafe public override void Update()
		{
			double* destination;
			FValueOut.GetValuePointer(out destination);
			
			if (FSliceCount > 0)
				Marshal.Copy(FData, 0, new IntPtr(destination), FData.Length);
			
			base.Update();
		}
	}
}
