using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Config;

namespace VVVV.PluginInterfaces.V2
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
			return null;
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
