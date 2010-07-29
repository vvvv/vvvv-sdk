using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// T is one of:
	/// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
	/// </summary>
	public abstract class ValueInputPin<T> : InputPin<T> where T: struct
	{
		protected IValueIn FValueIn;
		protected double[] FData;
		
		public ValueInputPin(IPluginHost host, InputAttribute attribute)
		{
			FData = new double[1];
			
			var type = typeof(T);
			
			double minValue, maxValue, stepSize;
			bool isInteger = true;
			
			LoadDefaultValues(type, attribute, out minValue, out maxValue, out stepSize, out isInteger);
			
			host.CreateValueInput(attribute.Name, 1, null, attribute.SliceMode, attribute.Visibility, out FValueIn);
			FValueIn.SetSubType(minValue, maxValue, stepSize, attribute.DefaultValue, false, false, isInteger);
		}
		
		public override IPluginIn PluginIn
		{
			get
			{
				return FValueIn;
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
					FData = new double[sliceCount];
				
				Marshal.Copy(new IntPtr(source), FData, 0, sliceCount);
			}
		}
	}
}
