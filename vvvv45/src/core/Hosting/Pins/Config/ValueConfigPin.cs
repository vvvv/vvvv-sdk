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
	public abstract class ValueConfigPin<T> : DiffValuePin<T>, IDiffSpread<T> where T: struct
	{
		protected IValueConfig FValueConfig;
		
		public ValueConfigPin(IPluginHost host, ConfigAttribute attribute, double minValue, double maxValue, double stepSize)
			: base(host, attribute, minValue, maxValue, stepSize)
		{
			host.CreateValueConfig(FName, 1, null, FSliceMode, FVisibility, out FValueConfig);
			FValueConfig.SetSubType(FMinValue, FMaxValue, FStepSize, FDefaultValue, FIsBang, FIsToggle, FIsInteger);
			base.InitializeInternalPin(FValueConfig);
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FValueConfig.SliceCount;
			}
			set 
			{
				base.SliceCount = value;
				
				if (FAttribute.SliceMode != SliceMode.Single)
					FValueConfig.SliceCount = FSliceCount;
			}
		}
		
		public override bool IsChanged 
		{
			get 
			{
				return FValueConfig.PinIsChanged;
			}
		}
	}
}
