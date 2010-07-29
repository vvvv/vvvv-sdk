using System;
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
		
		public ValueConfigPin(IPluginHost host, ConfigAttribute attribute)
		{
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			
			LoadDefaultValues(type, attribute, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueConfig(attribute.Name, 1, null, attribute.SliceMode, attribute.Visibility, out FValueConfig);
			FValueConfig.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, false, false, isInteger);
		}
		
		public override IPluginConfig PluginConfig
		{
			get
			{
				return FValueConfig;
			}
		}
	}
}
