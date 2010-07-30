using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class ValueConfigPin<T> : ConfigPin<T> where T: struct
	{
		protected IValueConfig FValueConfig;
		protected double[] FData;
		protected int FDimension;
		
		public ValueConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			
			LoadDefaultValues(type, attribute, out FDimension, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueConfig(attribute.Name, FDimension, null, attribute.SliceMode, attribute.Visibility, out FValueConfig);
			switch (FDimension)
			{
				case 2:
					FValueConfig.SetSubType2D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], false, false, isInteger);
					break;
				case 3:
					FValueConfig.SetSubType3D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], false, false, isInteger);
					break;
				case 4:
					FValueConfig.SetSubType4D(minValue, maxValue, stepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], false, false, isInteger);
					break;
				default:
					FValueConfig.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, false, false, isInteger);
					break;
			}
			
			FData = new double[FDimension * 1];
		}
		
		public override IPluginConfig PluginConfig
		{
			get
			{
				return FValueConfig;
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
			int sliceCount;
			double* source;
			
			FValueConfig.GetValuePointer(out sliceCount, out source);
			
			if (sliceCount != FData.Length)
				FData = new double[sliceCount * FDimension];
			
			Marshal.Copy(new IntPtr(source), FData, 0, sliceCount * FDimension);
		}
	}
}
