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
		
		public BinSpread(IPluginHost host, PinAttribute attribute)
		{
		}
		
		public abstract ISpread<T> this[int index]
		{
			get;
			set;
		}
		
		public abstract int SliceCount
		{
			get;
			set;
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
