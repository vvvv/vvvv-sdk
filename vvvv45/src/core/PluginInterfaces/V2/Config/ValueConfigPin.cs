using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class ValueConfigPin<T> : ConfigPin<T>, IPinUpdater where T: struct
	{
		protected IValueConfig FValueConfig;
		protected double[] FData;
		protected int FDimension;
		protected int FSliceCount;
		
		public ValueConfigPin(IPluginHost host, ConfigAttribute attribute)
			: base(host, attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			bool isBool = type == typeof(bool);
			
			LoadDefaultValues(type, attribute, out FDimension, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueConfig(attribute.Name, FDimension, attribute.DimensionNames, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FValueConfig);
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
					FValueConfig.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, isBool && attribute.IsBang, isBool && !attribute.IsBang, isInteger);
					break;
			}
			
			FValueConfig.SetPinUpdater(this);
			
			SliceCount = 1;
		}
		
		protected override IPluginConfig PluginConfig
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
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
					FData = new double[value * FDimension];
				
				FSliceCount = value;
				
				if (FAttribute.SliceMode != SliceMode.Single)
					FValueConfig.SliceCount = value;
			}
		}
		
		unsafe public override void Update()
		{
			int sliceCount;
			double* source;
			
			FValueConfig.GetValuePointer(out sliceCount, out source);
			
			var moduloResult = sliceCount % FDimension;
			if (moduloResult != 0)
				SliceCount = sliceCount / FDimension + 1;
			else
				SliceCount = sliceCount / FDimension;
			
			Marshal.Copy(new IntPtr(source), FData, 0, sliceCount);
			
			// Fill end of FData with values from start.
			Array.Copy(FData, 0, FData, sliceCount, FData.Length - sliceCount);
			
			base.Update();
		}
	}
}
