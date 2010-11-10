using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// Base class 2d spreads.
	/// </summary>
	public abstract class BinSpread<T> : ISpread<ISpread<T>>
	{
		private ISpread<ISpread<T>> FSpreads;
		
		public BinSpread(IPluginHost host, PinAttribute attribute)
		{
			FSpreads = new Spread<ISpread<T>>(1);
			FSpreads[0] = new Spread<T>(0);
		}
		
		public ISpread<T> this[int index]
		{
			get
			{
				return FSpreads[index];
			}
			set
			{
				FSpreads[index] = value;
			}
		}
		
		public int SliceCount
		{
			get
			{
				return FSpreads.SliceCount;
			}
			set
			{
				if (FSpreads.SliceCount != value)
				{
					int oldSliceCount = FSpreads.SliceCount;
					
					FSpreads.SliceCount = value;
					
					for (int i = oldSliceCount; i < FSpreads.SliceCount; i++)
					{
						FSpreads[i] = new Spread<T>(0);
					}
				}
			}
		}

		public IEnumerator<ISpread<T>> GetEnumerator()
		{
			for (int i = 0; i < SliceCount; i++)
				yield return this[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
