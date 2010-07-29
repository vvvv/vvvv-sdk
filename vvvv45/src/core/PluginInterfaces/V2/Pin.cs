using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class Pin<T> : ISpread<T>
	{
		public Pin()
		{
		}
		
		public abstract T this[int index]
		{
			get;
			set;
		}
		
		public abstract int SliceCount
		{
			get;
			set;
		}
		
		public virtual void Update()
		{
			// DO nothing
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < SliceCount; i++)
				yield return this[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		protected void LoadDefaultValues(Type type, PinAttribute attribute, out int dimension, out double minValue, out double maxValue, out double stepSize, out bool isInteger)
		{
			dimension = 1;
			minValue = attribute.MinValue;
			maxValue = attribute.MaxValue;
			stepSize = attribute.StepSize;
			isInteger = true;
			
			if (type == typeof(Vector2D))
				dimension = 2;
			else if (type == typeof(Vector3D))
				dimension = 3;
			else if (type == typeof(Vector4D))
				dimension = 4;
			
			if (minValue == PinAttribute.DefaultMinValue)
			{
				if (type == typeof(bool))
					minValue = 0.0;
				else if (type == typeof(byte))
					minValue = byte.MinValue;
				else if (type == typeof(sbyte))
					minValue = sbyte.MinValue;
				else if (type == typeof(int))
					minValue = int.MinValue;
				else if (type == typeof(uint))
					minValue = uint.MinValue;
				else if (type == typeof(short))
					minValue = short.MinValue;
				else if (type == typeof(ushort))
					minValue = ushort.MinValue;
				else if (type == typeof(long))
					minValue = long.MinValue;
				else if (type == typeof(ulong))
					minValue = ulong.MinValue;
				else if (type == typeof(float))
					minValue = float.MinValue;
				else if (type == typeof(double))
					minValue = double.MinValue;
			}
			
			if (maxValue == PinAttribute.DefaultMaxValue)
			{
				if (type == typeof(bool))
					maxValue = 1.0;
				else if (type == typeof(byte))
					maxValue = byte.MaxValue;
				else if (type == typeof(sbyte))
					maxValue = sbyte.MaxValue;
				else if (type == typeof(int))
					maxValue = int.MaxValue;
				else if (type == typeof(uint))
					maxValue = uint.MaxValue;
				else if (type == typeof(short))
					maxValue = short.MaxValue;
				else if (type == typeof(ushort))
					maxValue = ushort.MaxValue;
				else if (type == typeof(long))
					maxValue = long.MaxValue;
				else if (type == typeof(ulong))
					maxValue = ulong.MaxValue;
				else if (type == typeof(float))
					maxValue = float.MaxValue;
				else if (type == typeof(double))
					maxValue = double.MaxValue;
			}
			
			if (stepSize == PinAttribute.DefaultStepSize)
			{
				if (type == typeof(float))
					stepSize = 0.1;
				else if (type == typeof(double))
					stepSize = 0.1;
			}
			
			if (type == typeof(float))
				isInteger = false;
			else if (type == typeof(double))
				isInteger = false;
		}
	}
}
