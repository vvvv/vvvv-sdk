using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class ValueOutputPin<T> : OutputPin<T> where T: struct
	{
		protected IValueOut FValueOut;
		protected double[] FData;
		
		public ValueOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			FData = new double[1];
			
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			
			LoadDefaultValues(type, attribute, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueOutput(attribute.Name, 1, null, attribute.SliceMode, attribute.Visibility, out FValueOut);
			FValueOut.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, false, false, isInteger);
		}
		
		public override IPluginOut PluginOut
		{
			get
			{
				return FValueOut;
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
					FData = new double[value];
				
				base.SliceCount = value;
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
