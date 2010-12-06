using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// Base class for ValueInputPin and ValueOutputPin
	/// </summary>
	public abstract class ValuePin<T> : Pin<T> where T: struct
	{
		protected double FMinValue;
		protected double FMaxValue;
		protected double FStepSize;
		protected double FDefaultValue;
		protected bool FIsBang;
		protected bool FIsToggle;
		protected bool FIsInteger;
		
		public ValuePin(IPluginHost host, PinAttribute attribute, double minValue, double maxValue, double stepSize)
			: base(host, attribute)
		{
			FMinValue = minValue;
			if (attribute.MinValue != PinAttribute.DefaultMinValue)
				FMinValue = attribute.MinValue;
			
			FMaxValue = maxValue;
			if (attribute.MaxValue != PinAttribute.DefaultMaxValue)
				FMaxValue = attribute.MaxValue;
			
			FStepSize = stepSize;
			if (attribute.StepSize != PinAttribute.DefaultStepSize)
				FStepSize = attribute.StepSize;
			else if (attribute.AsInt)
				FStepSize = 1.0;
			
			var isBool = typeof(T) == typeof(bool);
			var isInteger = typeof(T) == typeof(int);
			
			FIsBang = isBool && attribute.IsBang;
			FIsToggle = isBool && !attribute.IsBang;
			FIsInteger = isInteger || attribute.AsInt;
		}
	}
	
	/// <summary>
	/// Base class for DiffValueInputPin and ValueConfigPin
	/// </summary>
	public abstract class DiffValuePin<T> : DiffPin<T> where T: struct
	{
		protected double FMinValue;
		protected double FMaxValue;
		protected double FStepSize;
		protected double FDefaultValue;
		protected bool FIsBang;
		protected bool FIsToggle;
		protected bool FIsInteger;
		
		public DiffValuePin(IPluginHost host, PinAttribute attribute, double minValue, double maxValue, double stepSize)
			: base(host, attribute)
		{
			FMinValue = minValue;
			if (attribute.MinValue != PinAttribute.DefaultMinValue)
				FMinValue = attribute.MinValue;
			
			FMaxValue = maxValue;
			if (attribute.MaxValue != PinAttribute.DefaultMaxValue)
				FMaxValue = attribute.MaxValue;
			
			FStepSize = stepSize;
			if (attribute.StepSize != PinAttribute.DefaultStepSize)
				FStepSize = attribute.StepSize;
			else if (attribute.AsInt)
				FStepSize = 1.0;
			
			var isBool = typeof(T) == typeof(bool);
			var isInteger = typeof(T) == typeof(int);
			
			FIsBang = isBool && attribute.IsBang;
			FIsToggle = isBool && !attribute.IsBang;
			FIsInteger = isInteger || attribute.AsInt;
		}
	}
}
