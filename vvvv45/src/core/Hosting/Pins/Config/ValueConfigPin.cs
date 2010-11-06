using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class ValueConfigPin<T> : ConfigPin<T> where T: struct
	{
		protected IValueConfig FValueConfig;
		protected int FDimension;
		
		public ValueConfigPin(IPluginHost host, ConfigAttribute attribute)
			: base(host, attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			bool isBool = type == typeof(bool);
			
			PinUtils.LoadDefaultValues(type, attribute, out FDimension, out minValue, out maxValue, out stepSize, out isInteger);
			
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
			
			base.Initialize(FValueConfig);
		}
	}
}
